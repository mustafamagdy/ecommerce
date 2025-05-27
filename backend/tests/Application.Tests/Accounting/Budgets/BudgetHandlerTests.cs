using FluentAssertions;
using FSH.WebApi.Application.Accounting.Budgets;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts; // For IAggregateRoot and BaseEntity
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Mapster;
using Xunit;
using FluentValidation;

namespace FSH.WebApi.Application.Tests.Accounting.Budgets;

public class BudgetHandlerTests
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;

    private readonly IStringLocalizer<CreateBudgetHandler> _createLocalizer;
    private readonly ILogger<CreateBudgetHandler> _createLogger;
    private readonly IStringLocalizer<UpdateBudgetHandler> _updateLocalizer;
    private readonly ILogger<UpdateBudgetHandler> _updateLogger;
    private readonly IStringLocalizer<GetBudgetHandler> _getLocalizer;
    private readonly IStringLocalizer<DeleteBudgetHandler> _deleteLocalizer;
    private readonly ILogger<DeleteBudgetHandler> _deleteLogger;
    private readonly IStringLocalizer<SearchBudgetsHandler> _searchLocalizer;

    public BudgetHandlerTests()
    {
        _budgetRepository = Substitute.For<IRepository<Budget>>();
        _accountRepository = Substitute.For<IRepository<Account>>();
        _transactionRepository = Substitute.For<IRepository<Transaction>>();

        _createLocalizer = Substitute.For<IStringLocalizer<CreateBudgetHandler>>();
        _createLogger = Substitute.For<ILogger<CreateBudgetHandler>>();
        _updateLocalizer = Substitute.For<IStringLocalizer<UpdateBudgetHandler>>();
        _updateLogger = Substitute.For<ILogger<UpdateBudgetHandler>>();
        _getLocalizer = Substitute.For<IStringLocalizer<GetBudgetHandler>>();
        _deleteLocalizer = Substitute.For<IStringLocalizer<DeleteBudgetHandler>>();
        _deleteLogger = Substitute.For<ILogger<DeleteBudgetHandler>>();
        _searchLocalizer = Substitute.For<IStringLocalizer<SearchBudgetsHandler>>();

        // Setup default localization messages
        SetupLocalizationMock(_createLocalizer);
        SetupLocalizationMock(_updateLocalizer);
        SetupLocalizationMock(_getLocalizer);
        SetupLocalizationMock(_deleteLocalizer);
        SetupLocalizationMock(_searchLocalizer);
    }

    private void SetupLocalizationMock(IStringLocalizer localizer)
    {
        localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
    }

    private Account CreateMockAccount(Guid id, string name, string number, AccountType type, decimal balance = 0m, bool isActive = true)
    {
        var acc = new Account(name, number, type, balance, string.Empty, isActive);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(acc, id);
        return acc;
    }
     private Budget CreateMockBudget(Guid id, Guid accountId, string name, decimal amount, DateTime startDate, DateTime endDate)
    {
        var budget = new Budget(name, accountId, startDate, endDate, amount, "Test Budget Desc");
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(budget, id);
        return budget;
    }

    private JournalEntry CreateMockJournalEntry(Guid id, DateTime entryDate, bool isPosted = true, DateTime? postedDate = null)
    {
        var je = new JournalEntry(entryDate, "Test JE", "REF001");
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(je, id);
        if (isPosted)
        {
            je.GetType().GetProperty(nameof(JournalEntry.IsPosted))!.SetValue(je, true);
            je.GetType().GetProperty(nameof(JournalEntry.PostedDate))!.SetValue(je, postedDate ?? entryDate);
        }
        return je;
    }
    private Transaction CreateMockTransaction(Guid id, JournalEntry je, Account acc, TransactionType type, decimal amount)
    {
        var trans = new Transaction(je.Id, acc.Id, type, amount, "Test Transaction");
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(trans, id);
        trans.JournalEntry = je;
        trans.Account = acc;
        return trans;
    }

    // === CreateBudgetHandler Tests ===
    [Fact]
    public async Task CreateBudgetHandler_Should_Create_And_Return_BudgetId_When_Request_Is_Valid()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new CreateBudgetRequest
        {
            BudgetName = "Test Budget", AccountId = accountId, Amount = 1000,
            PeriodStartDate = DateTime.UtcNow.Date, PeriodEndDate = DateTime.UtcNow.Date.AddMonths(1)
        };
        var account = CreateMockAccount(accountId, "Expense Account", "E001", AccountType.Expense, isActive: true);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        Budget? capturedBudget = null;
        await _budgetRepository.AddAsync(Arg.Do<Budget>(b => capturedBudget = b), Arg.Any<CancellationToken>());

        var handler = new CreateBudgetHandler(_budgetRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _budgetRepository.Received(1).AddAsync(Arg.Any<Budget>(), Arg.Any<CancellationToken>());
        capturedBudget.Should().NotBeNull();
        capturedBudget!.Id.Should().Be(result);
        capturedBudget.BudgetName.Should().Be(request.BudgetName);
    }

    [Fact]
    public async Task CreateBudgetHandler_Should_Throw_NotFoundException_When_AccountId_Does_Not_Exist()
    {
        // Arrange
        var request = new CreateBudgetRequest { AccountId = Guid.NewGuid(), BudgetName = "Test", Amount = 100, PeriodStartDate = DateTime.UtcNow, PeriodEndDate = DateTime.UtcNow.AddDays(1) };
        _accountRepository.GetByIdAsync(request.AccountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(null));
        var handler = new CreateBudgetHandler(_budgetRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateBudgetHandler_Should_Throw_ValidationException_When_Account_Is_Not_Active()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new CreateBudgetRequest { AccountId = accountId, /* other properties */ };
        var inactiveAccount = CreateMockAccount(accountId, "Inactive Account", "I001", AccountType.Asset, isActive: false);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(inactiveAccount));
        var handler = new CreateBudgetHandler(_budgetRepository, _accountRepository, _createLocalizer, _createLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === UpdateBudgetHandler Tests ===
    [Fact]
    public async Task UpdateBudgetHandler_Should_Update_Budget_When_Request_Is_Valid()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var request = new UpdateBudgetRequest
        {
            Id = budgetId, BudgetName = "Updated Budget", AccountId = accountId, Amount = 1500,
            PeriodStartDate = DateTime.UtcNow.Date, PeriodEndDate = DateTime.UtcNow.Date.AddMonths(2)
        };
        var existingBudget = CreateMockBudget(budgetId, Guid.NewGuid(), "Old Budget", 1000, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        var account = CreateMockAccount(accountId, "Expense Account", "E001", AccountType.Expense, isActive: true);

        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(existingBudget));
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        var handler = new UpdateBudgetHandler(_budgetRepository, _accountRepository, _updateLocalizer, _updateLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(budgetId);
        await _budgetRepository.Received(1).UpdateAsync(Arg.Is<Budget>(b =>
            b.Id == budgetId &&
            b.BudgetName == request.BudgetName &&
            b.Amount == request.Amount
        ), Arg.Any<CancellationToken>());
    }

     [Fact]
    public async Task UpdateBudgetHandler_Should_Throw_NotFoundException_When_Budget_Not_Found()
    {
        var request = new UpdateBudgetRequest { Id = Guid.NewGuid(), /* other properties */ };
        _budgetRepository.GetByIdAsync(request.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(null));
        var handler = new UpdateBudgetHandler(_budgetRepository, _accountRepository, _updateLocalizer, _updateLogger);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateBudgetHandler_Should_Throw_NotFoundException_When_Account_Not_Found_On_Update()
    {
        var budgetId = Guid.NewGuid();
        var request = new UpdateBudgetRequest { Id = budgetId, AccountId = Guid.NewGuid(), /* other properties */ };
        var existingBudget = CreateMockBudget(budgetId, Guid.NewGuid(), "Old Budget", 1000, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(existingBudget));
        _accountRepository.GetByIdAsync(request.AccountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(null));
        var handler = new UpdateBudgetHandler(_budgetRepository, _accountRepository, _updateLocalizer, _updateLogger);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetBudgetHandler Tests ===
    [Fact]
    public async Task GetBudgetHandler_Should_Return_BudgetDto_With_Correct_ActualAmount_And_Variance_For_Expense_Account()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);
        var budgetAmount = 1000m;

        var budget = CreateMockBudget(budgetId, accountId, "Expense Budget", budgetAmount, startDate, endDate);
        var account = CreateMockAccount(accountId, "Office Supplies", "E002", AccountType.Expense);

        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(budget));
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        var je1 = CreateMockJournalEntry(Guid.NewGuid(), startDate.AddDays(5));
        var je2 = CreateMockJournalEntry(Guid.NewGuid(), startDate.AddDays(10));
        // Mock transactions for the current budget period (ActualAmount = 200)
        var transactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), je1, account, TransactionType.Debit, 150m), // Expense: +150
            CreateMockTransaction(Guid.NewGuid(), je2, account, TransactionType.Debit, 50m)   // Expense: +50
        }; // Total Actual Expense = 200m

        _transactionRepository.ListAsync(Arg.Any<TransactionsForAccountInPeriodSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(transactions));

        var handler = new GetBudgetHandler(_budgetRepository, _accountRepository, _transactionRepository, _getLocalizer);
        var request = new GetBudgetRequest(budgetId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(budgetId);
        result.Amount.Should().Be(budgetAmount);
        result.ActualAmount.Should().Be(200m); // 150 + 50
        result.Variance.Should().Be(800m); // 1000 - 200
        result.AccountName.Should().Be("Office Supplies");
    }

    [Fact]
    public async Task GetBudgetHandler_Should_Throw_NotFoundException_If_Budget_Not_Found()
    {
        var budgetId = Guid.NewGuid();
        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(null));
        var handler = new GetBudgetHandler(_budgetRepository, _accountRepository, _transactionRepository, _getLocalizer);
        var request = new GetBudgetRequest(budgetId);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === DeleteBudgetHandler Tests ===
    [Fact]
    public async Task DeleteBudgetHandler_Should_Call_DeleteAsync_When_Budget_Exists()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = CreateMockBudget(budgetId, Guid.NewGuid(), "ToDelete", 100, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(budget));
        var handler = new DeleteBudgetHandler(_budgetRepository, _deleteLocalizer, _deleteLogger);
        var request = new DeleteBudgetRequest(budgetId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(budgetId);
        await _budgetRepository.Received(1).DeleteAsync(budget, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteBudgetHandler_Should_Throw_NotFoundException_When_Budget_Not_Found()
    {
        var budgetId = Guid.NewGuid();
        _budgetRepository.GetByIdAsync(budgetId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Budget?>(null));
        var handler = new DeleteBudgetHandler(_budgetRepository, _deleteLocalizer, _deleteLogger);
        var request = new DeleteBudgetRequest(budgetId);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === SearchBudgetsHandler Tests ===
    [Fact]
    public async Task SearchBudgetsHandler_Should_Return_PaginationResponse_With_Calculated_Amounts()
    {
        // Arrange
        var request = new SearchBudgetsRequest { PageNumber = 1, PageSize = 10 };
        var accountId1 = Guid.NewGuid();
        var budget1 = CreateMockBudget(Guid.NewGuid(), accountId1, "Budget 1", 500, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        var account1 = CreateMockAccount(accountId1, "Expense Acc 1", "E001", AccountType.Expense);

        var budgetsList = new List<Budget> { budget1 };
        var budgetDtos = budgetsList.Adapt<List<BudgetDto>>(); // Initial map for structure

        _budgetRepository.ListAsync(Arg.Any<ISpecification<Budget, BudgetDto>>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(budgetDtos)); // Mapster will map Budget to BudgetDto
        _budgetRepository.CountAsync(Arg.Any<ISpecification<Budget, BudgetDto>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(budgetsList.Count));

        _accountRepository.GetByIdAsync(accountId1, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account1));

        // Mock transactions for budget1 (ActualAmount = 50)
        var jeBudget1 = CreateMockJournalEntry(Guid.NewGuid(), budget1.PeriodStartDate.AddDays(2));
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == accountId1), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), jeBudget1, account1, TransactionType.Debit, 50m) }));

        var handler = new SearchBudgetsHandler(_budgetRepository, _accountRepository, _transactionRepository, _searchLocalizer);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data[0].BudgetName.Should().Be("Budget 1");
        result.Data[0].Amount.Should().Be(500m);
        result.Data[0].ActualAmount.Should().Be(50m);
        result.Data[0].Variance.Should().Be(450m); // 500 - 50
        result.Data[0].AccountName.Should().Be("Expense Acc 1");
    }
}

// Minimal Spec implementations for testing Budget handlers
// Actual specs might be more complex and reside in the Application layer.
internal class BudgetsBySearchFilterSpec : Specification<Budget, BudgetDto>
{
    public BudgetsBySearchFilterSpec(SearchBudgetsRequest request)
        : base() // Using base() instead of : base(request) as EntitiesByPaginationFilterSpec is not used here
    {
        Query.OrderBy(b => b.BudgetName, !request.HasOrderBy());

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Where(b => b.BudgetName.Contains(request.Keyword));
        }
        if (request.AccountId.HasValue)
        {
            Query.Where(b => b.AccountId == request.AccountId.Value);
        }
        // Omitting date filtering for this minimal spec example for simplicity
        if (request.PageNumber > 0 && request.PageSize > 0)
        {
             Query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
        }
    }
}

// Added TransactionsForAccountInPeriodSpec to resolve the AccountId error
internal class TransactionsForAccountInPeriodSpec : Specification<Transaction>
{
    public Guid AccountId { get; }
    public DateTime FromDate { get; }
    public DateTime ToDate { get; }
    
    public TransactionsForAccountInPeriodSpec(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        AccountId = accountId;
        FromDate = fromDate;
        ToDate = toDate;
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate >= fromDate && t.JournalEntry.PostedDate < toDate.AddDays(1))
            .Include(t => t.JournalEntry)
            .OrderBy(t => t.JournalEntry.PostedDate);
    }
}
