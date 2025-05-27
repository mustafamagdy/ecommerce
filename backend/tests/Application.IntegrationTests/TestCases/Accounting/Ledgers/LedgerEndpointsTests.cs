using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra; // For TestFixture, HostFixture
using FluentAssertions;
using FSH.WebApi.Application.Accounting.Accounts; // For AccountDto, CreateAccountRequest
using FSH.WebApi.Application.Accounting.JournalEntries; // For CreateJournalEntryRequest
using FSH.WebApi.Application.Accounting.Ledgers; // For GetAccountLedgerRequest, AccountLedgerDto
using FSH.WebApi.Domain.Accounting; // For AccountType, TransactionType enums
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Accounting.Ledgers;

public class LedgerEndpointsTests : TestFixture
{
    private Dictionary<string, string> _adminHeaders;
    private Guid _branchId;

    public LedgerEndpointsTests(HostFixture host, ITestOutputHelper output)
        : base(host, output)
    {
    }

    // Helper to create an Account via API for test setup
    private async Task<AccountDto> CreateAccountAsync(string name, string number, AccountType type, decimal initialBalance = 0, Dictionary<string, string>? authHeaders = null)
    {
        var request = new CreateAccountRequest
        {
            AccountName = name,
            AccountNumber = number,
            AccountType = type.ToString(),
            InitialBalance = initialBalance,
            Description = $"Test account {name}"
        };
        var response = await PostAsJsonAsync("/api/v1/accounting/accounts", request, authHeaders ?? _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accountId = await response.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await GetAsync($"/api/v1/accounting/accounts/{accountId}", authHeaders ?? _adminHeaders);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return await getResponse.Content.ReadFromJsonAsync<AccountDto>();
    }

    // Helper to create and post a Journal Entry via API
    private async Task<Guid> CreateAndPostJournalEntryAsync(CreateJournalEntryRequest request, Dictionary<string, string>? authHeaders = null)
    {
        var headers = authHeaders ?? _adminHeaders;
        // Create JE
        var createResponse = await PostAsJsonAsync("/api/v1/accounting/journal-entries", request, headers);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var journalEntryId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Post JE
        var postResponse = await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{journalEntryId}/post", null, headers);
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return journalEntryId;
    }

    [Fact]
    public async Task GetAccountLedger_Should_Return_Correct_Ledger_Details()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        
        var cashAccount = await CreateAccountAsync("Cash Ledger Test", "CASHLEDGER01", AccountType.Asset, 1000m, _adminHeaders); // Initial balance 1000

        // Transactions before the period (Opening Balance calculation)
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 1, 10), Description = "OB Debit",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = "Debit", Amount = 200m }, // Bal: 1000 + 200 = 1200
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("OB Contra", "OBCONTRA", AccountType.Expense, authHeaders: _adminHeaders)).Id, TransactionType = "Credit", Amount = 200m }
            }
        }, _adminHeaders);
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 1, 15), Description = "OB Credit",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = "Credit", Amount = 50m }, // Bal: 1200 - 50 = 1150
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("OB Contra 2", "OBCONTRA2", AccountType.Revenue, authHeaders: _adminHeaders)).Id, TransactionType = "Debit", Amount = 50m }
            }
        }, _adminHeaders);
        // Expected Opening Balance for Feb 1st = 1150

        // Transactions within the period
        var ledgerStartDate = new DateTime(2023, 2, 1);
        var ledgerEndDate = new DateTime(2023, 2, 28);

        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 2, 5), Description = "Period Debit 1",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = "Debit", Amount = 300m }, // Bal: 1150 + 300 = 1450
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Period Contra 1", "PCONTRA1", AccountType.Revenue, authHeaders: _adminHeaders)).Id, TransactionType = "Credit", Amount = 300m }
            }
        }, _adminHeaders);
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 2, 15), Description = "Period Credit 1",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = "Credit", Amount = 100m }, // Bal: 1450 - 100 = 1350
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Period Contra 2", "PCONTRA2", AccountType.Expense, authHeaders: _adminHeaders)).Id, TransactionType = "Debit", Amount = 100m }
            }
        }, _adminHeaders);
        // Expected Closing Balance = 1350

        // Transactions after the period (should not affect ledger)
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 3, 1), Description = "Post Period Debit",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = "Debit", Amount = 500m },
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Post Period Contra", "POSTCONTRA", AccountType.Revenue, authHeaders: _adminHeaders)).Id, TransactionType = "Credit", Amount = 500m }
            }
        }, _adminHeaders);

        var ledgerRequest = new GetAccountLedgerRequest(cashAccount.Id, ledgerStartDate, ledgerEndDate);

        // Act
        var response = await PostAsJsonAsync<GetAccountLedgerRequest>("/api/v1/accounting/ledgers/account-statement", ledgerRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ledgerDto = await response.Content.ReadFromJsonAsync<AccountLedgerDto>();

        ledgerDto.Should().NotBeNull();
        ledgerDto!.AccountId.Should().Be(cashAccount.Id);
        ledgerDto.AccountName.Should().Be(cashAccount.AccountName);
        ledgerDto.OpeningBalance.Should().Be(1150m);
        ledgerDto.ClosingBalance.Should().Be(1350m);

        ledgerDto.Entries.Should().HaveCount(2);
        ledgerDto.Entries[0].Description.Should().Be("Period Debit 1");
        ledgerDto.Entries[0].DebitAmount.Should().Be(300m);
        ledgerDto.Entries[0].CreditAmount.Should().Be(0);
        ledgerDto.Entries[0].Balance.Should().Be(1150m + 300m); // 1450m

        ledgerDto.Entries[1].Description.Should().Be("Period Credit 1");
        ledgerDto.Entries[1].DebitAmount.Should().Be(0);
        ledgerDto.Entries[1].CreditAmount.Should().Be(100m);
        ledgerDto.Entries[1].Balance.Should().Be(1450m - 100m); // 1350m
    }

    [Fact]
    public async Task GetAccountLedger_Should_Return_NotFound_For_NonExistent_Account()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        
        var nonExistentAccountId = Guid.NewGuid();
        var ledgerRequest = new GetAccountLedgerRequest(nonExistentAccountId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Act
        var response = await PostAsJsonAsync<GetAccountLedgerRequest>("/api/v1/accounting/ledgers/account-statement", ledgerRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountLedger_Should_Return_Empty_Entries_And_Correct_Balances_For_Account_With_No_Transactions_In_Period()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        
        var account = await CreateAccountAsync("No Period Trans Ledger", "NOPERIOD01", AccountType.Asset, 500m, _adminHeaders); // Initial Balance 500

        // Transaction before the period
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2023, 1, 20), Description = "OB Only",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = account.Id, TransactionType = "Debit", Amount = 100m }, // Bal: 500 + 100 = 600
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("OB Contra NoPeriod", "OBCNP", AccountType.Expense, authHeaders: _adminHeaders)).Id, TransactionType = "Credit", Amount = 100m }
            }
        }, _adminHeaders);
        // Expected Opening Balance for Feb 1st = 600

        var ledgerStartDate = new DateTime(2023, 2, 1);
        var ledgerEndDate = new DateTime(2023, 2, 28);
        var ledgerRequest = new GetAccountLedgerRequest(account.Id, ledgerStartDate, ledgerEndDate);

        // Act
        var response = await PostAsJsonAsync<GetAccountLedgerRequest>("/api/v1/accounting/ledgers/account-statement", ledgerRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ledgerDto = await response.Content.ReadFromJsonAsync<AccountLedgerDto>();

        ledgerDto.Should().NotBeNull();
        ledgerDto!.AccountId.Should().Be(account.Id);
        ledgerDto.OpeningBalance.Should().Be(600m);
        ledgerDto.ClosingBalance.Should().Be(600m); // No transactions in period, so closing = opening
        ledgerDto.Entries.Should().BeEmpty();
    }
}
