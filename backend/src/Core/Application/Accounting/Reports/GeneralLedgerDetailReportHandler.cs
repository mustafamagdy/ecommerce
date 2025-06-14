using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.JournalEntries.Specifications; // For the new specs
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class GeneralLedgerDetailReportHandler : IRequestHandler<GeneralLedgerDetailReportRequest, GeneralLedgerDetailReportDto>
{
    private readonly IReadRepository<Account> _accountRepo;
    private readonly IReadRepository<JournalEntry> _journalEntryRepo;
    // private readonly IStringLocalizer<GeneralLedgerDetailReportHandler> _localizer; // Optional

    public GeneralLedgerDetailReportHandler(
        IReadRepository<Account> accountRepo,
        IReadRepository<JournalEntry> journalEntryRepo
        /* IStringLocalizer<GeneralLedgerDetailReportHandler> localizer */)
    {
        _accountRepo = accountRepo;
        _journalEntryRepo = journalEntryRepo;
        // _localizer = localizer;
    }

    public async Task<GeneralLedgerDetailReportDto> Handle(GeneralLedgerDetailReportRequest request, CancellationToken cancellationToken)
    {
        // 1. Fetch Account Details
        var account = await _accountRepo.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            // Use nameof(request.AccountId) for paramName if localizer not used, or a generic message
            throw new NotFoundException($"Account with ID {request.AccountId} not found.");
        }

        var reportDto = new GeneralLedgerDetailReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AccountId = account.Id,
            AccountCode = account.AccountNumber, // Assuming AccountNumber is the Code
            AccountName = account.AccountName,
            AccountType = account.AccountType.ToString(),
            Lines = new List<GeneralLedgerDetailReportLineDto>(),
            GeneratedOn = DateTime.UtcNow.ToString("o")
        };

        // 2. Calculate Opening Balance
        decimal openingBalance = 0m;
        var openingBalanceSpec = new JournalEntriesBeforeDateForAccountSpec(request.AccountId, request.StartDate);
        var openingJournalEntries = await _journalEntryRepo.ListAsync(openingBalanceSpec, cancellationToken);

        foreach (var je in openingJournalEntries)
        {
            foreach (var transaction in je.Transactions.Where(t => t.AccountId == request.AccountId))
            {
                openingBalance += transaction.Debit;
                openingBalance -= transaction.Credit;
            }
        }
        reportDto.OpeningBalance = openingBalance;

        // 3. Fetch Transactions for the Period
        var periodSpec = new JournalEntriesInPeriodForAccountSpec(request.AccountId, request.StartDate, request.EndDate);
        var periodJournalEntries = await _journalEntryRepo.ListAsync(periodSpec, cancellationToken);

        // 4. Populate Report DTO
        decimal currentRunningBalance = openingBalance;
        decimal totalDebitsInPeriod = 0m;
        decimal totalCreditsInPeriod = 0m;

        // Flatten transactions and sort them by JE date then JE creation time (done in spec)
        // then by transaction ID for deterministic order within a JE if multiple lines hit same account
        var periodTransactionsForAccount = periodJournalEntries
            .SelectMany(je => je.Transactions
                .Where(t => t.AccountId == request.AccountId)
                .Select(t => new { JournalEntry = je, Transaction = t })) // Keep JE context
            .OrderBy(x => x.JournalEntry.Date)
            .ThenBy(x => x.JournalEntry.CreatedOn) // From AuditableEntity of JournalEntry
            .ThenBy(x => x.Transaction.Id) // Fallback for multiple transactions to same account in same JE
            .ToList();


        foreach (var item in periodTransactionsForAccount)
        {
            var je = item.JournalEntry;
            var transaction = item.Transaction;

            currentRunningBalance += transaction.Debit;
            currentRunningBalance -= transaction.Credit;

            var line = new GeneralLedgerDetailReportLineDto
            {
                Date = je.Date,
                JournalEntryId = je.Id,
                JournalEntryNumber = je.EntryNumber ?? je.Id.ToString().Substring(0,8), // Assuming JE has EntryNumber or use part of Id
                TransactionId = transaction.Id,
                Description = transaction.Description ?? je.Description, // Prefer transaction desc, fallback to JE desc
                Reference = je.Reference,
                DebitAmount = transaction.Debit,
                CreditAmount = transaction.Credit,
                RunningBalance = currentRunningBalance
            };
            reportDto.Lines.Add(line);

            totalDebitsInPeriod += transaction.Debit;
            totalCreditsInPeriod += transaction.Credit;
        }

        reportDto.TotalDebitsInPeriod = totalDebitsInPeriod;
        reportDto.TotalCreditsInPeriod = totalCreditsInPeriod;
        reportDto.ClosingBalance = currentRunningBalance; // The final running balance is the closing balance

        return reportDto;
    }
}
