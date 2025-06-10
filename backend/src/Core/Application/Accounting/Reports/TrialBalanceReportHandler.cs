using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.JournalEntries.Specifications; // For JournalEntriesUpToDateSpec
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class TrialBalanceReportHandler : IRequestHandler<TrialBalanceReportRequest, TrialBalanceReportDto>
{
    private readonly IReadRepository<Account> _accountRepo;
    private readonly IReadRepository<JournalEntry> _journalEntryRepo;
    // private readonly IStringLocalizer<TrialBalanceReportHandler> _localizer; // Optional

    public TrialBalanceReportHandler(
        IReadRepository<Account> accountRepo,
        IReadRepository<JournalEntry> journalEntryRepo
        /* IStringLocalizer<TrialBalanceReportHandler> localizer */)
    {
        _accountRepo = accountRepo;
        _journalEntryRepo = journalEntryRepo;
        // _localizer = localizer;
    }

    public async Task<TrialBalanceReportDto> Handle(TrialBalanceReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new TrialBalanceReportDto
        {
            ReportDate = request.EndDate,
            // StartDate = request.StartDate, // If using StartDate
            Lines = new List<TrialBalanceReportLineDto>(),
            GeneratedOn = DateTime.UtcNow.ToString("o") // Moved here from DTO default for consistency
        };

        // 1. Fetch all Accounts
        // TODO: Consider specification if accounts need filtering (e.g., only active accounts)
        var allAccounts = await _accountRepo.ListAsync(cancellationToken);
        if (allAccounts == null || !allAccounts.Any())
        {
            // No accounts, return empty report
            return reportDto;
        }

        // 2. Fetch all relevant Journal Entries (and their Transactions) up to the EndDate
        var journalEntriesSpec = new JournalEntriesUpToDateSpec(request.EndDate);
        var journalEntries = await _journalEntryRepo.ListAsync(journalEntriesSpec, cancellationToken);

        // 3. Aggregate transactions by AccountId for efficiency if there are many JEs
        // This creates a dictionary where Key is AccountId and Value is a list of transactions for that account.
        var transactionsByAccount = journalEntries
            .SelectMany(je => je.Transactions) // Flatten all transactions from all journal entries
            .GroupBy(t => t.AccountId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 4. Iterate through each GL Account to calculate its balance
        foreach (var account in allAccounts)
        {
            decimal totalAccountDebits = 0m;
            decimal totalAccountCredits = 0m;

            if (transactionsByAccount.TryGetValue(account.Id, out var accountTransactions))
            {
                foreach (var transaction in accountTransactions)
                {
                    totalAccountDebits += transaction.Debit;
                    totalAccountCredits += transaction.Credit;
                }
            }

            // Determine final balance and whether it's debit or credit
            decimal balance = totalAccountDebits - totalAccountCredits;
            decimal debitBalance = 0m;
            decimal creditBalance = 0m;

            // Consider the natural balance of the account type.
            // Assets & Expenses normally have Debit balances.
            // Liabilities, Equity & Revenue normally have Credit balances.
            // A positive 'balance' for an Asset means it's a DebitBalance.
            // A positive 'balance' for a Liability means it's a CreditBalance (so balance variable would be negative).
            // The current calculation (Debit - Credit) gives a positive result for net debit, negative for net credit.

            if (balance > 0) // Net Debit
            {
                debitBalance = balance;
            }
            else if (balance < 0) // Net Credit
            {
                creditBalance = -balance; // Store as positive value in credit column
            }
            // If balance is 0, both debitBalance and creditBalance remain 0.

            // Only add lines with activity or non-zero balance.
            // Or, based on requirements, always show all accounts. For Trial Balance, typically all accounts are shown.
            // if (debitBalance != 0 || creditBalance != 0 || totalAccountDebits != 0 || totalAccountCredits != 0)
            // {
                var line = new TrialBalanceReportLineDto
                {
                    AccountId = account.Id,
                    AccountCode = account.AccountNumber, // Assuming AccountNumber is the Code
                    AccountName = account.AccountName,
                    AccountType = account.AccountType.ToString(), // Enum to string
                    DebitBalance = debitBalance,
                    CreditBalance = creditBalance
                };
                reportDto.Lines.Add(line);
            // }
        }

        // 5. Calculate total debits and credits for the report
        reportDto.TotalDebits = reportDto.Lines.Sum(l => l.DebitBalance);
        reportDto.TotalCredits = reportDto.Lines.Sum(l => l.CreditBalance);

        // 6. Sort lines by AccountCode
        reportDto.Lines = reportDto.Lines.OrderBy(l => l.AccountCode).ToList();

        return reportDto;
    }
}
