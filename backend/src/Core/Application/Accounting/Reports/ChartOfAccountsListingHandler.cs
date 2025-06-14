using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.Accounts.Specifications;     // For AccountsWithFilterSpec, AccountsByIdsSpec
using FSH.WebApi.Application.Accounting.JournalEntries.Specifications; // For JournalEntriesUpToDateSpec (or similar for all JEs)

namespace FSH.WebApi.Application.Accounting.Reports;

public class ChartOfAccountsListingHandler : IRequestHandler<ChartOfAccountsListingRequest, ChartOfAccountsListingDto>
{
    private readonly IReadRepository<Account> _accountRepo;
    private readonly IReadRepository<JournalEntry> _journalEntryRepo; // To calculate balances

    public ChartOfAccountsListingHandler(
        IReadRepository<Account> accountRepo,
        IReadRepository<JournalEntry> journalEntryRepo)
    {
        _accountRepo = accountRepo;
        _journalEntryRepo = journalEntryRepo;
    }

    public async Task<ChartOfAccountsListingDto> Handle(ChartOfAccountsListingRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new ChartOfAccountsListingDto
        {
            FilterIsActive = request.IsActive,
            GeneratedOn = DateTime.UtcNow // Set generation time directly
        };

        // 1. Fetch Accounts (potentially filtered)
        var accountsSpec = new AccountsWithFilterSpec(request.IsActive);
        var accounts = await _accountRepo.ListAsync(accountsSpec, cancellationToken);

        if (accounts == null || !accounts.Any())
        {
            return reportDto; // Return empty report if no accounts match filter
        }

        // 2. Prepare Parent Account Details
        var parentAccountIds = accounts
            .Where(a => a.ParentAccountId.HasValue)
            .Select(a => a.ParentAccountId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, Account> parentAccountsDict = new Dictionary<Guid, Account>();
        if (parentAccountIds.Any())
        {
            var parentAccountsSpec = new AccountsByIdsSpec(parentAccountIds);
            var parentAccounts = await _accountRepo.ListAsync(parentAccountsSpec, cancellationToken);
            parentAccountsDict = parentAccounts.ToDictionary(pa => pa.Id);
        }

        // 3. Calculate Balances
        // Fetch all transactions. For CoA, typically "as of now" or all time.
        // If a specific date is needed for balances, the request/logic would need an "AsOfDate".
        // Using JournalEntriesUpToDateSpec with a very future date or max date to get all.
        // Or, a new spec like AllJournalEntriesSpec.
        var allJournalEntriesSpec = new JournalEntriesUpToDateSpec(DateTime.MaxValue); // Gets all JEs
        var journalEntries = await _journalEntryRepo.ListAsync(allJournalEntriesSpec, cancellationToken);

        var accountBalances = journalEntries
            .SelectMany(je => je.Transactions)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Balance = g.Sum(t => t.Debit - t.Credit)
            })
            .ToDictionary(gb => gb.AccountId, gb => gb.Balance);

        // 4. Populate Report DTO Lines and determine levels (basic parent/child for now)
        var accountLines = new List<ChartOfAccountsListingLineDto>();
        var accountLevelMap = new Dictionary<Guid, int>(); // To store calculated levels

        // First pass: determine levels for accounts with no parents (level 0)
        foreach (var account in accounts.Where(a => !a.ParentAccountId.HasValue))
        {
            accountLevelMap[account.Id] = 0;
        }

        // Iteratively determine levels for child accounts (simple one-pass for direct children)
        // For multi-level hierarchy, a recursive function or multiple passes would be needed.
        // This loop helps if accounts are not perfectly ordered by parent-child.
        bool levelsChanged;
        do
        {
            levelsChanged = false;
            foreach (var account in accounts)
            {
                if (account.ParentAccountId.HasValue && accountLevelMap.ContainsKey(account.ParentAccountId.Value))
                {
                    if (!accountLevelMap.ContainsKey(account.Id)) // if level not yet set
                    {
                        accountLevelMap[account.Id] = accountLevelMap[account.ParentAccountId.Value] + 1;
                        levelsChanged = true;
                    }
                }
                else if (!account.ParentAccountId.HasValue && !accountLevelMap.ContainsKey(account.Id))
                {
                     accountLevelMap[account.Id] = 0; // Should have been caught above, but as fallback
                     levelsChanged = true;
                }
            }
        }
        while (levelsChanged); // Repeat if new levels were assigned, for deeper hierarchies

        foreach (var account in accounts)
        {
            parentAccountsDict.TryGetValue(account.ParentAccountId ?? Guid.Empty, out var parentAccount);
            accountBalances.TryGetValue(account.Id, out var balance);

            accountLines.Add(new ChartOfAccountsListingLineDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber, // Using AccountNumber as Code
                AccountName = account.AccountName,
                AccountType = account.AccountType.ToString(),
                IsActive = account.IsActive,
                Description = account.Description, // Using Description as Notes
                ParentAccountId = account.ParentAccountId,
                ParentAccountCode = parentAccount?.AccountNumber,
                ParentAccountName = parentAccount?.AccountName,
                Level = accountLevelMap.TryGetValue(account.Id, out int level) ? level : (account.ParentAccountId.HasValue ? 1 : 0), // Fallback level
                Balance = balance
            });
        }

        // Sort accounts: typically by AccountNumber (Code) which might reflect hierarchy
        reportDto.Accounts = accountLines.OrderBy(a => a.AccountNumber).ToList();
        // For true hierarchical sorting, would need to sort by path/level then code.

        return reportDto;
    }
}
