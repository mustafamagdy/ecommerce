using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.BankAccounts.Specifications;
using FSH.WebApi.Application.Accounting.BankStatements.Specifications;
using FSH.WebApi.Application.Accounting.JournalEntries.Specifications;
using FSH.WebApi.Application.Common.Exceptions;
// using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.Accounting.Reports;

public class OutstandingTransactionsReportHandler : IRequestHandler<OutstandingTransactionsReportRequest, OutstandingTransactionsReportDto>
{
    private readonly IReadRepository<BankAccount> _bankAccountRepo;
    private readonly IReadRepository<Account> _glAccountRepo;
    private readonly IReadRepository<BankStatementTransaction> _statementTransactionRepo;
    private readonly IReadRepository<JournalEntry> _journalEntryRepo;
    // private readonly IStringLocalizer<OutstandingTransactionsReportHandler> _localizer;

    public OutstandingTransactionsReportHandler(
        IReadRepository<BankAccount> bankAccountRepo,
        IReadRepository<Account> glAccountRepo,
        IReadRepository<BankStatementTransaction> statementTransactionRepo,
        IReadRepository<JournalEntry> journalEntryRepo
        /* IStringLocalizer<OutstandingTransactionsReportHandler> localizer */)
    {
        _bankAccountRepo = bankAccountRepo;
        _glAccountRepo = glAccountRepo;
        _statementTransactionRepo = statementTransactionRepo;
        _journalEntryRepo = journalEntryRepo;
        // _localizer = localizer;
    }

    public async Task<OutstandingTransactionsReportDto> Handle(OutstandingTransactionsReportRequest request, CancellationToken cancellationToken)
    {
        // 1. Fetch Bank Account & GL Account
        var bankAccountSpec = new BankAccountWithGLByIdSpec(request.BankAccountId); // This spec fetches BankAccount
        var bankAccount = await _bankAccountRepo.FirstOrDefaultAsync(bankAccountSpec, cancellationToken);
        if (bankAccount == null)
            throw new NotFoundException($"Bank Account with ID {request.BankAccountId} not found.");

        var glAccount = await _glAccountRepo.GetByIdAsync(bankAccount.GLAccountId, cancellationToken);
        if (glAccount == null)
            throw new NotFoundException($"GL Account for Bank Account {bankAccount.AccountName} (ID: {bankAccount.GLAccountId}) not found.");

        // 2. Initialize Report DTO
        var reportDto = new OutstandingTransactionsReportDto
        {
            BankAccountId = bankAccount.Id,
            BankAccountName = bankAccount.AccountName,
            BankAccountNumber = bankAccount.AccountNumber,
            AsOfDate = request.AsOfDate,
            TypeFilterApplied = request.TypeFilter.ToString(),
            LookbackWindowDaysUsed = request.LookbackWindowDays,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Transactions = new List<OutstandingTransactionLineDto>()
        };

        // 3. Get All Reconciled System Transaction IDs for this Bank Account up to AsOfDate
        var reconciledSystemTxIdsSpec = new ReconciledSystemTransactionIdsForAccountSpec(request.BankAccountId, request.AsOfDate);
        var reconciledSystemTxIds = (await _statementTransactionRepo.ListAsync(reconciledSystemTxIdsSpec, cancellationToken)).ToHashSet();

        // 4. Fetch Candidate GL Transactions
        DateTime windowStartDate = request.AsOfDate.AddDays(-request.LookbackWindowDays);
        var glTransactionsSpec = new JournalEntriesForGLAccountInDateWindowSpec(bankAccount.GLAccountId, windowStartDate, request.AsOfDate);
        var journalEntriesInWindow = await _journalEntryRepo.ListAsync(glTransactionsSpec, cancellationToken);

        var systemTransactionsForGLAccount = journalEntriesInWindow
            .SelectMany(je => je.Transactions
                .Where(t => t.AccountId == bankAccount.GLAccountId) // Double check, though spec should handle
                .Select(t => new { JournalEntry = je, Transaction = t }))
            .OrderBy(x => x.JournalEntry.Date)
            .ThenBy(x => x.JournalEntry.CreatedOn) // Consistent ordering
            .ThenBy(x => x.Transaction.Id)
            .ToList();

        // 5. Identify Outstanding Transactions and Populate Lines
        reportDto.TotalAmount = 0m;
        reportDto.TotalUnclearedChecksOrDebitsAmount = 0m;
        reportDto.TotalDepositsInTransitOrCreditsAmount = 0m;

        foreach (var item in systemTransactionsForGLAccount)
        {
            var glTransaction = item.Transaction;
            var journalEntry = item.JournalEntry;

            // If this GL transaction's ID has been reconciled (i.e., appeared on a bank statement and matched) up to AsOfDate, skip it.
            if (reconciledSystemTxIds.Contains(glTransaction.Id))
            {
                continue;
            }

            string transactionTypeString = string.Empty;
            string amountTypeString = string.Empty;
            bool isDebitFromGL = glTransaction.Debit > 0; // From the perspective of the GL Bank Account

            if (isDebitFromGL) // System Debit (e.g., payment, withdrawal from bank account)
            {
                transactionTypeString = "Uncleared Check/Debit";
                amountTypeString = "Debit";
                if (request.TypeFilter == OutstandingTransactionTypeFilter.DepositsInTransitOrCredits) continue;
                reportDto.TotalUnclearedChecksOrDebitsAmount += glTransaction.Debit;
            }
            else // System Credit (e.g., deposit, receipt into bank account)
            {
                transactionTypeString = "Deposit in Transit/Credit";
                amountTypeString = "Credit";
                if (request.TypeFilter == OutstandingTransactionTypeFilter.UnclearedChecksOrDebits) continue;
                reportDto.TotalDepositsInTransitOrCreditsAmount += glTransaction.Credit;
            }

            var line = new OutstandingTransactionLineDto
            {
                SystemTransactionId = glTransaction.Id,
                TransactionDate = journalEntry.Date,
                TransactionType = transactionTypeString,
                JournalEntryNumber = journalEntry.EntryNumber ?? journalEntry.Id.ToString().Substring(0,8),
                Reference = journalEntry.Reference,
                Description = glTransaction.Description ?? journalEntry.Description,
                Amount = isDebitFromGL ? glTransaction.Debit : glTransaction.Credit,
                AmountType = amountTypeString,
                DaysOutstanding = (request.AsOfDate.Date - journalEntry.Date.Date).Days
            };
            reportDto.Transactions.Add(line);
            reportDto.TotalAmount += line.Amount;
        }

        // Sort report lines
        reportDto.Transactions = reportDto.Transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.Amount).ToList();

        return reportDto;
    }
}
