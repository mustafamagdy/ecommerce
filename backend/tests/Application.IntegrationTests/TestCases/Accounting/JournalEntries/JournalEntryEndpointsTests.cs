using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra; // For TestFixture, HostFixture
using FluentAssertions;
using FSH.WebApi.Application.Accounting.Accounts; // For AccountDto, CreateAccountRequest
using FSH.WebApi.Application.Accounting.JournalEntries;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Domain.Accounting; // For AccountType, TransactionType enums
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Accounting.JournalEntries;

public class JournalEntryEndpointsTests : TestFixture
{
    private Dictionary<string, string> _adminHeaders;
    private Guid _branchId;

    public JournalEntryEndpointsTests(HostFixture host, ITestOutputHelper output)
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
        
        // Retrieve the created account to get its DTO (including ID and initial balance)
        var getResponse = await GetAsync($"/api/v1/accounting/accounts/{accountId}", authHeaders ?? _adminHeaders);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return await getResponse.Content.ReadFromJsonAsync<AccountDto>();
    }
    
    private async Task<Guid> CreateJournalEntryViaApi(CreateJournalEntryRequest request, Dictionary<string, string>? authHeaders = null)
    {
        var response = await PostAsJsonAsync("/api/v1/accounting/journal-entries", request, authHeaders ?? _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Or Created, depends on API design
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<JournalEntryDto?> GetJournalEntryAsync(Guid id, Dictionary<string, string>? authHeaders = null)
    {
        var response = await GetAsync($"/api/v1/accounting/journal-entries/{id}", authHeaders ?? _adminHeaders);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JournalEntryDto>();
    }

    private async Task<AccountDto?> GetAccountAsync(Guid id, Dictionary<string, string>? authHeaders = null)
    {
        var response = await GetAsync($"/api/v1/accounting/accounts/{id}", authHeaders ?? _adminHeaders);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<AccountDto>();
    }


    [Fact]
    public async Task Can_Create_JournalEntry_When_Submit_Valid_Data()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var cashAccount = await CreateAccountAsync("Test Cash JE", "CASHJE001", AccountType.Asset, 1000, _adminHeaders);
        var revenueAccount = await CreateAccountAsync("Test Revenue JE", "REVJE001", AccountType.Revenue, 0, _adminHeaders);

        var createRequest = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Test Sales JE",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = TransactionType.Debit.ToString(), Amount = 200, Description = "Cash received" },
                new CreateTransactionRequestItem { AccountId = revenueAccount.Id, TransactionType = TransactionType.Credit.ToString(), Amount = 200, Description = "Sales revenue" }
            }
        };

        // Act
        var response = await PostAsJsonAsync("/api/v1/accounting/journal-entries", createRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var journalEntryId = await response.Content.ReadFromJsonAsync<Guid>();
        journalEntryId.Should().NotBeEmpty();

        var createdJeDto = await GetJournalEntryAsync(journalEntryId, _adminHeaders);
        createdJeDto.Should().NotBeNull();
        createdJeDto!.Description.Should().Be(createRequest.Description);
        createdJeDto.IsPosted.Should().BeFalse();
        createdJeDto.Transactions.Should().HaveCount(2);
        createdJeDto.Transactions.Should().ContainSingle(t => t.AccountId == cashAccount.Id && t.Amount == 200 && t.TransactionType == TransactionType.Debit.ToString());
        createdJeDto.Transactions.Should().ContainSingle(t => t.AccountId == revenueAccount.Id && t.Amount == 200 && t.TransactionType == TransactionType.Credit.ToString());
    }

    [Fact]
    public async Task Create_JournalEntry_Should_Return_BadRequest_When_Transactions_Unbalanced()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var cashAccount = await CreateAccountAsync("Test Cash Unbal", "CASHUB001", AccountType.Asset, 0, _adminHeaders);

        var createRequest = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Unbalanced JE",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = TransactionType.Debit.ToString(), Amount = 200 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 190 } // Unbalanced
            }
        };
         // Note: The second AccountId (Guid.NewGuid()) will also cause a NotFound if checked before balance validation.
         // The validator should check for balanced transactions first or also ensure accounts exist.
         // Assuming balance check is a primary validation rule hit.

        // Act
        var response = await PostAsJsonAsync("/api/v1/accounting/journal-entries", createRequest, _adminHeaders);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // Optionally check error message if API returns structured validation errors
        // var error = await response.Content.ReadFromJsonAsync<ErrorResult>();
        // error.Messages.Should().ContainMatch("*Debits must equal Credits*");
    }
    
    [Fact]
    public async Task Create_JournalEntry_Should_Return_NotFound_When_AccountId_In_Transaction_Not_Found()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var nonExistentAccountId = Guid.NewGuid();

        var createRequest = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "JE with non-existent account",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = nonExistentAccountId, TransactionType = TransactionType.Debit.ToString(), Amount = 100 },
                new CreateTransactionRequestItem { AccountId = Guid.NewGuid(), TransactionType = TransactionType.Credit.ToString(), Amount = 100 }
            }
        };

        // Act
        var response = await PostAsJsonAsync("/api/v1/accounting/journal-entries", createRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Or BadRequest depending on how handler/validator bubbles this up
    }


    [Fact]
    public async Task Can_Post_JournalEntry_And_Verify_Account_Balance_Changes()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var assetAccount = await CreateAccountAsync("Test Asset Post", "ASSETPOST01", AccountType.Asset, 1000m, _adminHeaders); // Initial Balance 1000
        var expenseAccount = await CreateAccountAsync("Test Expense Post", "EXPOST01", AccountType.Expense, 0m, _adminHeaders);    // Initial Balance 0

        var createJeRequest = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow.AddDays(-1),
            Description = "JE for Posting Test",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = expenseAccount.Id, TransactionType = TransactionType.Debit.ToString(), Amount = 150m }, // Expense increases
                new CreateTransactionRequestItem { AccountId = assetAccount.Id, TransactionType = TransactionType.Credit.ToString(), Amount = 150m }  // Asset (Cash) decreases
            }
        };
        var journalEntryId = await CreateJournalEntryViaApi(createJeRequest, _adminHeaders);

        // Act
        var postResponse = await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{journalEntryId}/post", null, _adminHeaders);

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var postedJournalEntryId = await postResponse.Content.ReadFromJsonAsync<Guid>();
        postedJournalEntryId.Should().Be(journalEntryId);

        var postedJeDto = await GetJournalEntryAsync(journalEntryId, _adminHeaders);
        postedJeDto.Should().NotBeNull();
        postedJeDto!.IsPosted.Should().BeTrue();
        postedJeDto.PostedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10)); // Allow some clock skew

        // Verify Account Balances
        var updatedAssetAccount = await GetAccountAsync(assetAccount.Id, _adminHeaders);
        updatedAssetAccount.Should().NotBeNull();
        updatedAssetAccount!.Balance.Should().Be(1000m - 150m); // 1000 - 150 = 850

        var updatedExpenseAccount = await GetAccountAsync(expenseAccount.Id, _adminHeaders);
        updatedExpenseAccount.Should().NotBeNull();
        updatedExpenseAccount!.Balance.Should().Be(0m + 150m); // 0 + 150 = 150
    }
    
    [Fact]
    public async Task Post_JournalEntry_Should_Return_NotFound_For_NonExistent_Id()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var nonExistentId = Guid.NewGuid();
        var postResponse = await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{nonExistentId}/post", null, _adminHeaders);
        postResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_JournalEntry_Should_Return_BadRequest_For_Already_Posted_Entry()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var cashAccount = await CreateAccountAsync("Cash For Posted JE", "CASHPOSTED01", AccountType.Asset, 0, _adminHeaders);
        var revenueAccount = await CreateAccountAsync("Revenue For Posted JE", "REVPOSTED01", AccountType.Revenue, 0, _adminHeaders);
        var createRequest = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow, Description = "JE to be posted twice",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = cashAccount.Id, TransactionType = TransactionType.Debit.ToString(), Amount = 50 },
                new CreateTransactionRequestItem { AccountId = revenueAccount.Id, TransactionType = TransactionType.Credit.ToString(), Amount = 50 }
            }
        };
        var journalEntryId = await CreateJournalEntryViaApi(createRequest, _adminHeaders);
        await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{journalEntryId}/post", null, _adminHeaders); // First post

        // Act: Attempt to post again
        var secondPostResponse = await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{journalEntryId}/post", null, _adminHeaders);

        // Assert
        secondPostResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Or Conflict (409)
    }


    [Fact]
    public async Task Can_Search_JournalEntries_By_DateRange_And_PostedStatus()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var acc1 = await CreateAccountAsync("SearchAcc1 JE", "SJEACC01", AccountType.Asset, 0, _adminHeaders);
        var acc2 = await CreateAccountAsync("SearchAcc2 JE", "SJEACC02", AccountType.Expense, 0, _adminHeaders);

        var je1Date = new DateTime(2023, 5, 10);
        var je2Date = new DateTime(2023, 5, 15);
        var je3Date = new DateTime(2023, 5, 20);

        var je1 = await CreateJournalEntryViaApi(new CreateJournalEntryRequest { EntryDate = je1Date, Description = "JE Search Test 1", Transactions = new List<CreateTransactionRequestItem> { new CreateTransactionRequestItem { AccountId = acc1.Id, TransactionType = "Debit", Amount = 10 }, new CreateTransactionRequestItem { AccountId = acc2.Id, TransactionType = "Credit", Amount = 10 } } }, _adminHeaders);
        var je2 = await CreateJournalEntryViaApi(new CreateJournalEntryRequest { EntryDate = je2Date, Description = "JE Search Test 2", Transactions = new List<CreateTransactionRequestItem> { new CreateTransactionRequestItem { AccountId = acc1.Id, TransactionType = "Debit", Amount = 20 }, new CreateTransactionRequestItem { AccountId = acc2.Id, TransactionType = "Credit", Amount = 20 } } }, _adminHeaders);
        var je3 = await CreateJournalEntryViaApi(new CreateJournalEntryRequest { EntryDate = je3Date, Description = "JE Search Test 3, Unposted", Transactions = new List<CreateTransactionRequestItem> { new CreateTransactionRequestItem { AccountId = acc1.Id, TransactionType = "Debit", Amount = 30 }, new CreateTransactionRequestItem { AccountId = acc2.Id, TransactionType = "Credit", Amount = 30 } } }, _adminHeaders);

        // Post JE1 and JE2
        (await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{je1}/post", null, _adminHeaders)).EnsureSuccessStatusCode();
        (await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{je2}/post", null, _adminHeaders)).EnsureSuccessStatusCode();
        // JE3 remains unposted

        // Act & Assert: Search for posted JEs in date range
        var searchRequest = new SearchJournalEntriesRequest { StartDate = new DateTime(2023, 5, 1), EndDate = new DateTime(2023, 5, 18), IsPosted = true, PageNumber = 1, PageSize = 10 };
        var response = await PostAsJsonAsync<SearchJournalEntriesRequest>("/api/v1/accounting/journal-entries/search", searchRequest, _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResult = await response.Content.ReadFromJsonAsync<PaginationResponse<JournalEntryDto>>();
        
        searchResult.Should().NotBeNull();
        searchResult!.Data.Should().HaveCount(2);
        searchResult.Data.Should().Contain(d => d.Id == je1);
        searchResult.Data.Should().Contain(d => d.Id == je2);

        // Act & Assert: Search for unposted JEs
        searchRequest.IsPosted = false;
        searchRequest.StartDate = null; searchRequest.EndDate = null; // Clear date range
        response = await PostAsJsonAsync<SearchJournalEntriesRequest>("/api/v1/accounting/journal-entries/search", searchRequest, _adminHeaders);
        searchResult = await response.Content.ReadFromJsonAsync<PaginationResponse<JournalEntryDto>>();
        
        searchResult.Should().NotBeNull();
        searchResult!.Data.Should().ContainSingle(d => d.Id == je3);
    }
}
