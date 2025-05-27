using FluentAssertions;
using FSH.WebApi.Application.Accounting.Ledgers;
using FSH.WebApi.Application.Common.Exceptions;
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
using Xunit;

namespace FSH.WebApi.Application.Tests.Accounting.Ledgers;

public class LedgerHandlerTests
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GetAccountLedgerHandler> _localizer;
    private readonly ILogger<GetAccountLedgerHandler> _logger;

    public LedgerHandlerTests()
    {
        _accountRepository = Substitute.For<IRepository<Account>>();
        _transactionRepository = Substitute.For<IRepository<Transaction>>();
        _localizer = Substitute.For<IStringLocalizer<GetAccountLedgerHandler>>();
        _logger = Substitute.For<ILogger<GetAccountLedgerHandler>>();

        // Setup default localization messages if needed
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
    }

    private Account CreateMockAccount(Guid id, string name, string number, AccountType type, decimal initialBalance = 0m)
    {
        var acc = new Account(name, number, type, initialBalance, string.Empty, true);
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
        trans.JournalEntry = je; // Set navigation property
        trans.Account = acc;     // Set navigation property
        return trans;
    }

    [Fact]
    public async Task GetAccountLedgerHandler_Should_Return_Correct_Ledger_When_Account_And_Transactions_Exist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 1, 31);
        var request = new GetAccountLedgerRequest(accountId, fromDate, toDate);

        var account = CreateMockAccount(accountId, "Test Asset Account", "ASSET001", AccountType.Asset);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        var jeOpening1 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(-2)); // Posted before FromDate
        var jeOpening2 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(-1)); // Posted before FromDate

        var jePeriod1 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(5));
        var jePeriod2 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(10));
        var jePeriod3 = CreateMockJournalEntry(Guid.NewGuid(), toDate.AddDays(1), isPosted: false); // Not posted, should be ignored
        var jePeriod4 = CreateMockJournalEntry(Guid.NewGuid(), toDate.AddDays(5)); // Posted after ToDate, should be ignored by period spec

        var openingTransactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), jeOpening1, account, TransactionType.Debit, 100m), // Asset: +100
            CreateMockTransaction(Guid.NewGuid(), jeOpening2, account, TransactionType.Credit, 30m) // Asset: -30
        }; // Opening Balance = 70

        var periodTransactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), jePeriod1, account, TransactionType.Debit, 50m),  // Asset: +50, Running Bal: 120
            CreateMockTransaction(Guid.NewGuid(), jePeriod2, account, TransactionType.Credit, 20m) // Asset: -20, Running Bal: 100
        };

        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountBeforeDateSpec>(s => s.AccountId == accountId && s.BeforeDate == fromDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(openingTransactions));

        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == accountId && s.FromDate == fromDate && s.ToDate == toDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(periodTransactions));

        var handler = new GetAccountLedgerHandler(_accountRepository, _transactionRepository, _localizer, _logger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(accountId);
        result.AccountName.Should().Be("Test Asset Account");
        result.OpeningBalance.Should().Be(70m); // 100 - 30
        result.ClosingBalance.Should().Be(100m); // 70 + 50 - 20

        result.Entries.Should().HaveCount(2);
        result.Entries[0].DebitAmount.Should().Be(50m);
        result.Entries[0].CreditAmount.Should().Be(0);
        result.Entries[0].Balance.Should().Be(120m); // 70 + 50

        result.Entries[1].DebitAmount.Should().Be(0);
        result.Entries[1].CreditAmount.Should().Be(20m);
        result.Entries[1].Balance.Should().Be(100m); // 120 - 20
    }

    [Fact]
    public async Task GetAccountLedgerHandler_Should_Throw_NotFoundException_When_Account_Not_Found()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new GetAccountLedgerRequest(accountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(null));
        var handler = new GetAccountLedgerHandler(_accountRepository, _transactionRepository, _localizer, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAccountLedgerHandler_Should_Return_Correct_Ledger_When_No_Transactions_In_Period()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 1, 31);
        var request = new GetAccountLedgerRequest(accountId, fromDate, toDate);

        var account = CreateMockAccount(accountId, "Test Account", "ACC001", AccountType.Expense);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        var jeOpening = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(-5));
        var openingTransactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), jeOpening, account, TransactionType.Debit, 150m) // Expense: +150
        }; // Opening Balance = 150

        var periodTransactions = new List<Transaction>(); // No transactions in period

        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountBeforeDateSpec>(s => s.AccountId == accountId && s.BeforeDate == fromDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(openingTransactions));
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == accountId && s.FromDate == fromDate && s.ToDate == toDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(periodTransactions));

        var handler = new GetAccountLedgerHandler(_accountRepository, _transactionRepository, _localizer, _logger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OpeningBalance.Should().Be(150m);
        result.ClosingBalance.Should().Be(150m); // Opening should equal closing
        result.Entries.Should().BeEmpty();
    }

     [Fact]
    public async Task GetAccountLedgerHandler_Should_Handle_LiabilityAccount_Correctly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 1, 31);
        var request = new GetAccountLedgerRequest(accountId, fromDate, toDate);

        var account = CreateMockAccount(accountId, "Test Liability Account", "LIAB001", AccountType.Liability);
        _accountRepository.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Account?>(account));

        var jeOpening = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(-2));
        var openingTransactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), jeOpening, account, TransactionType.Credit, 200m), // Liability: +200
        }; // Opening Balance = 200

        var jePeriod = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(5));
        var periodTransactions = new List<Transaction>
        {
            CreateMockTransaction(Guid.NewGuid(), jePeriod, account, TransactionType.Debit, 50m),  // Liability: -50, Running Bal: 150
        };

        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountBeforeDateSpec>(s => s.AccountId == accountId && s.BeforeDate == fromDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(openingTransactions));
        _transactionRepository.ListAsync(Arg.Is<TransactionsForAccountInPeriodSpec>(s => s.AccountId == accountId && s.FromDate == fromDate && s.ToDate == toDate), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<Transaction>>(periodTransactions));

        var handler = new GetAccountLedgerHandler(_accountRepository, _transactionRepository, _localizer, _logger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.OpeningBalance.Should().Be(200m);
        result.Entries.Should().HaveCount(1);
        result.Entries[0].DebitAmount.Should().Be(50m);
        result.Entries[0].CreditAmount.Should().Be(0);
        result.Entries[0].Balance.Should().Be(150m); // 200 - 50
        result.ClosingBalance.Should().Be(150m);
    }
}

// Minimal Spec implementations for testing, assuming the real ones are more complex
// and might be in Application layer. For unit testing handlers, these can be simplified.
internal class TransactionsForAccountBeforeDateSpec : Specification<Transaction>
{
    public Guid AccountId { get; }
    public DateTime BeforeDate { get; }
    public TransactionsForAccountBeforeDateSpec(Guid accountId, DateTime beforeDate)
    {
        AccountId = accountId;
        BeforeDate = beforeDate;
        Query.Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate < beforeDate)
             .Include(t => t.JournalEntry)
             .OrderBy(t => t.JournalEntry.PostedDate);
    }
}

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
        Query.Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate >= fromDate && t.JournalEntry.PostedDate < toDate.AddDays(1))
             .Include(t => t.JournalEntry)
             .OrderBy(t => t.JournalEntry.PostedDate).ThenBy(t => t.CreatedOn);
    }
}
