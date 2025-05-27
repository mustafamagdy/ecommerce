using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Mapster; // For Adapt

namespace FSH.WebApi.Application.Accounting.Ledgers;

public class GetAccountLedgerHandler : IRequestHandler<GetAccountLedgerRequest, AccountLedgerDto>
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer<GetAccountLedgerHandler> _localizer;
    private readonly ILogger<GetAccountLedgerHandler> _logger;

    public GetAccountLedgerHandler(
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IStringLocalizer<GetAccountLedgerHandler> localizer,
        ILogger<GetAccountLedgerHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<AccountLedgerDto> Handle(GetAccountLedgerRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException(_localizer["Account with ID {0} not found.", request.AccountId]);
        }

        // 1. Calculate OpeningBalance: Sum of all transactions for the account before FromDate.
        // This requires access to JournalEntry.EntryDate for transactions.
        // We need a specification that can filter transactions by AccountId and JournalEntry.EntryDate.
        var openingBalanceSpec = new TransactionsForAccountBeforeDateSpec(request.AccountId, request.FromDate);
        var openingTransactions = await _transactionRepository.ListAsync(openingBalanceSpec, cancellationToken);

        decimal openingBalance = 0;
        foreach (var trans in openingTransactions)
        {
            openingBalance += CalculateBalanceChange(trans.TransactionType, trans.Amount, account.AccountType);
        }
        // If the account itself has an initial balance from before any transactions, that should be the starting point.
        // For simplicity, we assume initial balance is 0 or already part of transactions.
        // More accurately, we could fetch the account's balance at a point in time if it's snapshot.
        // Given the current Account.Balance is "live", the sum of transactions is the way.

        // 2. Fetch Transactions for the account between FromDate and ToDate
        var periodTransactionsSpec = new TransactionsForAccountInPeriodSpec(request.AccountId, request.FromDate, request.ToDate);
        var periodTransactions = await _transactionRepository.ListAsync(periodTransactionsSpec, cancellationToken);

        var ledgerEntries = new List<AccountLedgerEntryDto>();
        decimal currentBalance = openingBalance;

        foreach (var trans in periodTransactions)
        {
            decimal debitAmount = 0;
            decimal creditAmount = 0;

            if (trans.TransactionType == TransactionType.Debit)
                debitAmount = trans.Amount;
            else
                creditAmount = trans.Amount;

            currentBalance += CalculateBalanceChange(trans.TransactionType, trans.Amount, account.AccountType);

            ledgerEntries.Add(new AccountLedgerEntryDto
            {
                EntryDate = trans.JournalEntry.EntryDate, // Assuming JournalEntry is loaded
                JournalEntryId = trans.JournalEntryId,
                Description = trans.Description ?? trans.JournalEntry.Description, // Fallback to JE description
                ReferenceNumber = trans.JournalEntry.ReferenceNumber,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount,
                Balance = currentBalance
            });
        }

        var accountLedgerDto = new AccountLedgerDto
        {
            AccountId = account.Id,
            AccountName = account.AccountName,
            AccountNumber = account.AccountNumber,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            OpeningBalance = openingBalance,
            ClosingBalance = currentBalance, // The last running balance is the closing balance
            Entries = ledgerEntries
        };

        _logger.LogInformation(_localizer["Account Ledger generated for Account ID: {0}"], request.AccountId);
        return accountLedgerDto;
    }

    private decimal CalculateBalanceChange(TransactionType transactionType, decimal amount, AccountType accountType)
    {
        // Determine if the transaction increases or decreases the balance based on account type
        bool isDebit = transactionType == TransactionType.Debit;
        switch (accountType)
        {
            case AccountType.Asset:
            case AccountType.Expense:
                return isDebit ? amount : -amount;
            case AccountType.Liability:
            case AccountType.Equity:
            case AccountType.Revenue:
                return isDebit ? -amount : amount;
            default:
                return 0; // Should not happen
        }
    }
}

// Specifications
public class TransactionsForAccountBeforeDateSpec : Specification<Transaction>
{
    public TransactionsForAccountBeforeDateSpec(Guid accountId, DateTime beforeDate)
    {
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate < beforeDate)
            .Include(t => t.JournalEntry) // Ensure JournalEntry is loaded for PostedDate and EntryDate
            .OrderBy(t => t.JournalEntry.PostedDate); // Order by posted date for correct balance calculation
    }
}

public class TransactionsForAccountInPeriodSpec : Specification<Transaction>
{
    public TransactionsForAccountInPeriodSpec(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        // Includes transactions on FromDate up to the end of ToDate
        Query
            .Where(t => t.AccountId == accountId && t.JournalEntry.IsPosted && t.JournalEntry.PostedDate >= fromDate && t.JournalEntry.PostedDate < toDate.AddDays(1))
            .Include(t => t.JournalEntry) // Ensure JournalEntry is loaded
            .OrderBy(t => t.JournalEntry.PostedDate).ThenBy(t => t.CreatedOn); // Order by PostedDate then by creation time as a tie-breaker
    }
}
