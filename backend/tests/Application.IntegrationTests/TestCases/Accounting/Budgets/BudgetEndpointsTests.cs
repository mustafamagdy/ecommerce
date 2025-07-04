using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra; // For TestFixture, HostFixture
using FluentAssertions;
using FSH.WebApi.Application.Accounting.Accounts; // For AccountDto, CreateAccountRequest
using FSH.WebApi.Application.Accounting.Budgets;
using FSH.WebApi.Application.Accounting.JournalEntries; // For CreateJournalEntryRequest
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Domain.Accounting; // For AccountType, TransactionType enums
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Accounting.Budgets;

public class BudgetEndpointsTests : TestFixture
{
    private Dictionary<string, string> _adminHeaders;
    private Guid _branchId;

    public BudgetEndpointsTests(HostFixture host, ITestOutputHelper output)
        : base(host, output)
    {
    }

    // Helper to create an Account via API
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
        var createResponse = await PostAsJsonAsync<CreateJournalEntryRequest>("/api/v1/accounting/journal-entries", request, headers);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var journalEntryId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var postResponse = await PostAsJsonAsync<object>($"/api/v1/accounting/journal-entries/{journalEntryId}/post", null, headers);
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return journalEntryId;
    }
    
    private async Task<Guid> CreateBudgetViaApi(CreateBudgetRequest request, Dictionary<string, string>? authHeaders = null)
    {
        var response = await PostAsJsonAsync<CreateBudgetRequest>("/api/v1/accounting/budgets", request, authHeaders ?? _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private async Task<BudgetDto?> GetBudgetViaApi(Guid id, Dictionary<string, string>? authHeaders = null)
    {
        var response = await GetAsync($"/api/v1/accounting/budgets/{id}", authHeaders ?? _adminHeaders);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<BudgetDto>();
    }

    [Fact]
    public async Task Can_Create_Budget_When_Submit_Valid_Data()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var expenseAccount = await CreateAccountAsync("Office Supplies Budget", "BUDGETEXP01", AccountType.Expense, authHeaders: _adminHeaders);
        var createRequest = new CreateBudgetRequest
        {
            BudgetName = "Q1 Office Supplies",
            AccountId = expenseAccount.Id,
            PeriodStartDate = new DateTime(2024, 1, 1),
            PeriodEndDate = new DateTime(2024, 3, 31),
            Amount = 1500,
            Description = "Budget for Q1 office supplies"
        };

        // Act
        var budgetId = await CreateBudgetViaApi(createRequest, _adminHeaders);

        // Assert
        budgetId.Should().NotBeEmpty();
        var budgetDto = await GetBudgetViaApi(budgetId, _adminHeaders);
        budgetDto.Should().NotBeNull();
        budgetDto!.BudgetName.Should().Be(createRequest.BudgetName);
        budgetDto.AccountId.Should().Be(createRequest.AccountId);
        budgetDto.Amount.Should().Be(createRequest.Amount);
        // ActualAmount and Variance will be tested in Get and Search tests
    }

    [Fact]
    public async Task Create_Budget_Should_Return_NotFound_For_NonExistent_AccountId()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var createRequest = new CreateBudgetRequest
        {
            BudgetName = "Invalid Acc Budget", AccountId = Guid.NewGuid(), // Non-existent
            PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddMonths(1), Amount = 100
        };
        var response = await PostAsJsonAsync<CreateBudgetRequest>("/api/v1/accounting/budgets", createRequest, _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Can_Get_Budget_By_Id_And_Verify_ActualAmount_And_Variance()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var account = await CreateAccountAsync("Supplies Expense GetTest", "EXPGET01", AccountType.Expense, authHeaders: _adminHeaders);
        var budgetStartDate = new DateTime(2024, 1, 1);
        var budgetEndDate = new DateTime(2024, 1, 31);
        var budgetAmount = 500m;

        var budgetId = await CreateBudgetViaApi(new CreateBudgetRequest
        {
            BudgetName = "January Supplies", AccountId = account.Id, PeriodStartDate = budgetStartDate,
            PeriodEndDate = budgetEndDate, Amount = budgetAmount
        }, _adminHeaders);

        // Post some transactions within the budget period
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = budgetStartDate.AddDays(5), Description = "Pens and Paper",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = account.Id, TransactionType = "Debit", Amount = 75m },
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Cash For BudgetGet", "CSHBG01", AccountType.Asset, 1000, _adminHeaders)).Id, TransactionType = "Credit", Amount = 75m }
            }
        }, _adminHeaders);
         await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest
        {
            EntryDate = budgetStartDate.AddDays(10), Description = "Staplers",
            Transactions = new List<CreateTransactionRequestItem>
            {
                new CreateTransactionRequestItem { AccountId = account.Id, TransactionType = "Debit", Amount = 25m },
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Cash For BudgetGet2", "CSHBG02", AccountType.Asset, 1000, _adminHeaders)).Id, TransactionType = "Credit", Amount = 25m }
            }
        }, _adminHeaders);
        // Expected ActualAmount = 75 + 25 = 100

        // Act
        var budgetDto = await GetBudgetViaApi(budgetId, _adminHeaders);

        // Assert
        budgetDto.Should().NotBeNull();
        budgetDto!.Amount.Should().Be(budgetAmount);
        budgetDto.ActualAmount.Should().Be(100m);
        budgetDto.Variance.Should().Be(budgetAmount - 100m); // 500 - 100 = 400
    }

    [Fact]
    public async Task Get_Budget_Should_Return_NotFound_For_NonExistent_Id()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var response = await GetAsync($"/api/v1/accounting/budgets/{Guid.NewGuid()}", _adminHeaders);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Can_Update_Budget()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var account = await CreateAccountAsync("Updatable Budget Acc", "UPDACCB01", AccountType.Expense, authHeaders: _adminHeaders);
        var budgetId = await CreateBudgetViaApi(new CreateBudgetRequest
        {
            BudgetName = "Old Budget Name", AccountId = account.Id, PeriodStartDate = DateTime.UtcNow,
            PeriodEndDate = DateTime.UtcNow.AddMonths(1), Amount = 300
        }, _adminHeaders);

        var updateRequest = new UpdateBudgetRequest
        {
            Id = budgetId, BudgetName = "New Budget Name", AccountId = account.Id, // AccountId can be changed too if needed
            PeriodStartDate = DateTime.UtcNow.AddDays(1), PeriodEndDate = DateTime.UtcNow.AddMonths(1).AddDays(1),
            Amount = 350, Description = "Updated Desc"
        };

        // Act
        var response = await PutAsJsonAsync($"/api/v1/accounting/budgets/{budgetId}", updateRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBudgetId = await response.Content.ReadFromJsonAsync<Guid>();
        updatedBudgetId.Should().Be(budgetId);

        var updatedBudgetDto = await GetBudgetViaApi(budgetId, _adminHeaders);
        updatedBudgetDto.Should().NotBeNull();
        updatedBudgetDto!.BudgetName.Should().Be(updateRequest.BudgetName);
        updatedBudgetDto.Amount.Should().Be(updateRequest.Amount);
        updatedBudgetDto.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task Can_Search_Budgets_And_Verify_Calculated_Fields()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var expenseAcc1 = await CreateAccountAsync("Search Exp Acc 1", "SEXP01", AccountType.Expense, authHeaders: _adminHeaders);
        var expenseAcc2 = await CreateAccountAsync("Search Exp Acc 2", "SEXP02", AccountType.Expense, authHeaders: _adminHeaders);

        var budget1Id = await CreateBudgetViaApi(new CreateBudgetRequest { BudgetName = "Marketing Q1", AccountId = expenseAcc1.Id, PeriodStartDate = new DateTime(2024,1,1), PeriodEndDate = new DateTime(2024,3,31), Amount = 10000 }, _adminHeaders);
        await CreateBudgetViaApi(new CreateBudgetRequest { BudgetName = "Office Q1", AccountId = expenseAcc2.Id, PeriodStartDate = new DateTime(2024,1,1), PeriodEndDate = new DateTime(2024,3,31), Amount = 2000 }, _adminHeaders);
        
        // Post transaction for Marketing Q1 budget
        await CreateAndPostJournalEntryAsync(new CreateJournalEntryRequest {
            EntryDate = new DateTime(2024,1,15), Description = "Ad Campaign",
            Transactions = new List<CreateTransactionRequestItem> {
                new CreateTransactionRequestItem { AccountId = expenseAcc1.Id, TransactionType = "Debit", Amount = 1200m },
                new CreateTransactionRequestItem { AccountId = (await CreateAccountAsync("Cash For SearchBudget", "CSHSB01", AccountType.Asset, 10000, _adminHeaders)).Id, TransactionType = "Credit", Amount = 1200m }
            }
        }, _adminHeaders);

        // Act: Search for "Marketing"
        var searchRequest = new SearchBudgetsRequest { Keyword = "Marketing", PageNumber = 1, PageSize = 10 };
        var response = await PostAsJsonAsync<SearchBudgetsRequest>("/api/v1/accounting/budgets/search", searchRequest, _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultSearch = await response.Content.ReadFromJsonAsync<PaginationResponse<BudgetDto>>();
        resultSearch.Should().NotBeNull();
        resultSearch!.Data.Should().ContainSingle(b => b.Id == budget1Id);
        var marketingBudgetDto = resultSearch.Data.First(b => b.Id == budget1Id);
        marketingBudgetDto.Amount.Should().Be(10000m);
        marketingBudgetDto.ActualAmount.Should().Be(1200m);
        marketingBudgetDto.Variance.Should().Be(10000m - 1200m);
    }

    [Fact]
    public async Task Can_Delete_Budget()
    {
        // Arrange
        var result = await CreateTenantAndLogin();
        _adminHeaders = result.Headers;
        _branchId = result.BranchId;
        var account = await CreateAccountAsync("Delete Budget Acc", "DELBUDACC01", AccountType.Expense, authHeaders: _adminHeaders);
        var budgetId = await CreateBudgetViaApi(new CreateBudgetRequest { BudgetName = "Budget To Delete", AccountId = account.Id, PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddMonths(1), Amount = 100 }, _adminHeaders);

        // Act
        var response = await DeleteAsJsonAsync($"/api/v1/accounting/budgets/{budgetId}", new DeleteBudgetRequest(budgetId), _adminHeaders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var deletedBudgetId = await response.Content.ReadFromJsonAsync<Guid>();
        deletedBudgetId.Should().Be(budgetId);

        var getResponse = await GetAsync($"/api/v1/accounting/budgets/{budgetId}", _adminHeaders);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
