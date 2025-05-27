using FluentAssertions;
using FSH.WebApi.Application.Accounting.FinancialStatements;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts; // For BaseEntity
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.FinancialStatements;

public class FinancialStatementHandlerTests
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GenerateProfitAndLossHandler> _pnlLocalizer;
    private readonly ILogger<GenerateProfitAndLossHandler> _pnlLogger;
    private readonly IStringLocalizer<GenerateBalanceSheetHandler> _bsLocalizer;
    private readonly ILogger<GenerateBalanceSheetHandler> _bsLogger;

    public FinancialStatementHandlerTests()
    {
        _accountRepository = Substitute.For<IRepository<Account>>();
        _transactionRepository = Substitute.For<IRepository<Transaction>>();
        _pnlLocalizer = Substitute.For<IStringLocalizer<GenerateProfitAndLossHandler>>();
        _pnlLogger = Substitute.For<ILogger<GenerateProfitAndLossHandler>>();
        _bsLocalizer = Substitute.For<IStringLocalizer<GenerateBalanceSheetHandler>>();
        _bsLogger = Substitute.For<ILogger<GenerateBalanceSheetHandler>>();
    }

    private Account CreateMockAccount(Guid id, string name, string number, AccountType type, decimal balance = 0m, bool isActive = true)
    {
        var acc = new Account(name, number, type, balance, string.Empty, isActive);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(acc, id);
        return acc;
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

    // === GenerateProfitAndLossHandler Tests ===
    [Fact]
    public async Task GenerateProfitAndLossHandler_Should_Calculate_Correctly_With_Revenue_And_Expenses()
    {
        // Arrange
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 1, 31);
        var request = new GenerateProfitAndLossRequest(fromDate, toDate);

        var revenueAcc1 = CreateMockAccount(Guid.NewGuid(), "Sales Revenue", "R001", AccountType.Revenue);
        var expenseAcc1 = CreateMockAccount(Guid.NewGuid(), "Rent Expense", "E001", AccountType.Expense);
        var expenseAcc2 = CreateMockAccount(Guid.NewGuid(), "COGS", "E002", AccountType.Expense); // Assuming COGS is an expense

        _accountRepository.ListAsync(Arg.Is<AccountsByTypeSpec>(s => s.AccountType == AccountType.Revenue), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(new List<Account> { revenueAcc1 }));
        _accountRepository.ListAsync(Arg.Is<AccountsByTypeSpec>(s => s.AccountType == AccountType.Expense), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(new List<Account> { expenseAcc1, expenseAcc2 }));

        var je1 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(5));
        var je2 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(10));
        var je3 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(15));

        // Transaction for revenue account
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == revenueAcc1.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction>
            {
                CreateMockTransaction(Guid.NewGuid(), je1, revenueAcc1, TransactionType.Credit, 500m), // Revenue +500
                CreateMockTransaction(Guid.NewGuid(), je3, revenueAcc1, TransactionType.Credit, 300m)  // Revenue +300
            }));

        // Transactions for expense accounts
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == expenseAcc1.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je2, expenseAcc1, TransactionType.Debit, 200m) }));

        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == expenseAcc2.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je2, expenseAcc2, TransactionType.Debit, 100m) }));

        var handler = new GenerateProfitAndLossHandler(_accountRepository, _transactionRepository, _pnlLocalizer, _pnlLogger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(800m);
        // Assuming COGS is part of OperatingExpenses for this basic setup as per handler logic
        result.TotalCostOfGoodsSold.Should().Be(100m); // Handler sets this to 0 explicitly
        result.TotalOperatingExpenses.Should().Be(300m); // 200 (Rent) + 100 (COGS)
        result.GrossProfit.Should().Be(800m); // TotalRevenue - TotalCOGS (100)
        result.NetProfit.Should().Be(500m);   // GrossProfit - TotalOperatingExpenses

        result.Revenue.Should().HaveCount(1);
        result.Revenue.First(r => r.AccountName == "Sales Revenue").Amount.Should().Be(800m);
        result.OperatingExpenses.Should().HaveCount(2);
        result.OperatingExpenses.First(e => e.AccountName == "Rent Expense").Amount.Should().Be(200m);
        result.OperatingExpenses.First(e => e.AccountName == "COGS").Amount.Should().Be(100m);
    }

    // === GenerateBalanceSheetHandler Tests ===
    [Fact]
    public async Task GenerateBalanceSheetHandler_Should_Calculate_Correctly_With_Various_Accounts()
    {
        // Arrange
        var asOfDate = new DateTime(2023, 1, 31);
        var request = new GenerateBalanceSheetRequest(asOfDate);

        var assetAcc = CreateMockAccount(Guid.NewGuid(), "Cash", "A001", AccountType.Asset);
        var liabAcc = CreateMockAccount(Guid.NewGuid(), "Accounts Payable", "L001", AccountType.Liability);
        var equityAcc = CreateMockAccount(Guid.NewGuid(), "Retained Earnings", "EQ001", AccountType.Equity);

        _accountRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Account>>(new List<Account> { assetAcc, liabAcc, equityAcc }));

        var je1 = CreateMockJournalEntry(Guid.NewGuid(), asOfDate.AddDays(-10));
        var je2 = CreateMockJournalEntry(Guid.NewGuid(), asOfDate.AddDays(-5));
        var je3 = CreateMockJournalEntry(Guid.NewGuid(), asOfDate.AddDays(-2));

        // Transaction for asset account
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == assetAcc.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction>
            {
                CreateMockTransaction(Guid.NewGuid(), je1, assetAcc, TransactionType.Debit, 1000m)
            }));

        // Transaction for liability account
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == liabAcc.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction>
            {
                CreateMockTransaction(Guid.NewGuid(), je1, liabAcc, TransactionType.Credit, 600m)
            }));

        // Transaction for equity account
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == equityAcc.Id), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(new List<Transaction>
            {
                CreateMockTransaction(Guid.NewGuid(), je1, equityAcc, TransactionType.Credit, 400m)
            }));

        var handler = new GenerateBalanceSheetHandler(_accountRepository, _bsLocalizer, _bsLogger, _transactionRepository);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAssets.Should().Be(1000m);
        result.TotalLiabilities.Should().Be(600m);
        result.TotalEquity.Should().Be(400m);
        result.TotalLiabilitiesAndEquity.Should().Be(1000m); // Assets = Liabilities + Equity

        result.Assets.Should().HaveCount(1);
        result.Assets.First().Amount.Should().Be(1000m);
        result.Liabilities.Should().HaveCount(1);
        result.Liabilities.First().Amount.Should().Be(600m);
        result.Equity.Should().HaveCount(1);
        result.Equity.First().Amount.Should().Be(400m);
    }
}

// Minimal Spec implementations for testing
internal class AccountsByTypeSpec : Specification<Account>
{
    public AccountType AccountType { get; }
    public AccountsByTypeSpec(AccountType accountType)
    {
        AccountType = accountType;
        Query.Where(a => a.AccountType == accountType && a.IsActive);
    }
}

// Re-using from LedgerHandlerTests or define locally if not shared.
// For this context, assuming it might be specific or slightly different.
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

internal class TransactionsForAccountUpToDateSpec : Specification<Transaction>
{
    public Guid AccountId { get; }
    public DateTime ToDate { get; }
    public TransactionsForAccountUpToDateSpec(Guid accountId, DateTime toDate)
    {
        AccountId = accountId;
        ToDate = toDate;
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate < toDate.AddDays(1)) // up to and including 'toDate'
            .Include(t => t.JournalEntry)
            .OrderBy(t => t.JournalEntry.PostedDate);
    }
}
