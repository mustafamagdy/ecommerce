using FluentAssertions;
using FSH.WebApi.Application.Accounting.FinancialStatements;
using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Ardalis.Specification;

namespace FSH.WebApi.Application.Tests.Accounting.FinancialStatements;

public class FinancialStatementReportHandlerTests
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IReadRepository<ProfitAndLossReport> _pnlTemplateRepository;
    private readonly IReadRepository<BalanceSheetReport> _bsTemplateRepository;
    private readonly IPdfWriter _pdfWriter;
    private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;
    private readonly IStringLocalizer<GenerateProfitAndLossReportHandler> _pnlLocalizer;
    private readonly ILogger<GenerateProfitAndLossReportHandler> _pnlLogger;
    private readonly IStringLocalizer<GenerateBalanceSheetReportHandler> _bsLocalizer;
    private readonly ILogger<GenerateBalanceSheetReportHandler> _bsLogger;

    public FinancialStatementReportHandlerTests()
    {
        _accountRepository = Substitute.For<IRepository<Account>>();
        _transactionRepository = Substitute.For<IRepository<Transaction>>();
        _pnlTemplateRepository = Substitute.For<IReadRepository<ProfitAndLossReport>>();
        _bsTemplateRepository = Substitute.For<IReadRepository<BalanceSheetReport>>();
        _pdfWriter = Substitute.For<IPdfWriter>();
        _subscriptionTypeResolver = Substitute.For<ISubscriptionTypeResolver>();
        _pnlLocalizer = Substitute.For<IStringLocalizer<GenerateProfitAndLossReportHandler>>();
        _pnlLogger = Substitute.For<ILogger<GenerateProfitAndLossReportHandler>>();
        _bsLocalizer = Substitute.For<IStringLocalizer<GenerateBalanceSheetReportHandler>>();
        _bsLogger = Substitute.For<ILogger<GenerateBalanceSheetReportHandler>>();

        _subscriptionTypeResolver.Resolve().Returns(SubscriptionType.Standard);
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

    [Fact]
    public async Task GenerateProfitAndLossReportHandler_Should_Return_Pdf_Stream()
    {
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 1, 31);
        var request = new GenerateProfitAndLossReportRequest(fromDate, toDate);

        var revenueAcc = CreateMockAccount(Guid.NewGuid(), "Sales", "R001", AccountType.Revenue);
        var expenseAcc = CreateMockAccount(Guid.NewGuid(), "Rent", "E001", AccountType.Expense);

        _accountRepository.ListAsync(Arg.Any<AccountsByTypeSpec>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new List<Account> { revenueAcc }),
                Task.FromResult(new List<Account> { expenseAcc }));

        var je1 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(5));
        var je2 = CreateMockJournalEntry(Guid.NewGuid(), fromDate.AddDays(6));

        _transactionRepository.ListAsync(Arg.Any<TransactionsForAccountInPeriodSpec>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je1, revenueAcc, TransactionType.Credit, 200m) }),
                Task.FromResult(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je2, expenseAcc, TransactionType.Debit, 50m) }));

        _pnlTemplateRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<ProfitAndLossReport>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ProfitAndLossReport?>(new ProfitAndLossReport()));

        var pdf = new MemoryStream();
        _pdfWriter.WriteToStream(Arg.Any<InvoiceDocument>()).Returns(pdf);

        var handler = new GenerateProfitAndLossReportHandler(
            _accountRepository,
            _transactionRepository,
            _pnlTemplateRepository,
            _pdfWriter,
            _subscriptionTypeResolver,
            _pnlLocalizer,
            _pnlLogger);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(pdf);
        _pdfWriter.Received(1).WriteToStream(Arg.Any<InvoiceDocument>());
    }

    [Fact]
    public async Task GenerateBalanceSheetReportHandler_Should_Return_Pdf_Stream()
    {
        var asOfDate = new DateTime(2023, 1, 31);
        var request = new GenerateBalanceSheetReportRequest(asOfDate);

        var assetAcc = CreateMockAccount(Guid.NewGuid(), "Cash", "A001", AccountType.Asset);
        var liabAcc = CreateMockAccount(Guid.NewGuid(), "AP", "L001", AccountType.Liability);
        var equityAcc = CreateMockAccount(Guid.NewGuid(), "Equity", "E001", AccountType.Equity);

        _accountRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Account> { assetAcc, liabAcc, equityAcc }));

        var je1 = CreateMockJournalEntry(Guid.NewGuid(), asOfDate.AddDays(-5));

        _transactionRepository.ListAsync(Arg.Any<TransactionsForAccountUpToDateSpec>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je1, assetAcc, TransactionType.Debit, 100m) }),
                Task.FromResult(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je1, liabAcc, TransactionType.Credit, 60m) }),
                Task.FromResult(new List<Transaction> { CreateMockTransaction(Guid.NewGuid(), je1, equityAcc, TransactionType.Credit, 40m) }));

        _bsTemplateRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BalanceSheetReport>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<BalanceSheetReport?>(new BalanceSheetReport()));

        var pdf = new MemoryStream();
        _pdfWriter.WriteToStream(Arg.Any<InvoiceDocument>()).Returns(pdf);

        var handler = new GenerateBalanceSheetReportHandler(
            _accountRepository,
            _transactionRepository,
            _bsTemplateRepository,
            _pdfWriter,
            _subscriptionTypeResolver,
            _bsLocalizer,
            _bsLogger);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(pdf);
        _pdfWriter.Received(1).WriteToStream(Arg.Any<InvoiceDocument>());
    }
}
