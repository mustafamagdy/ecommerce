using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For JournalEntry, Transaction
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.JournalEntries.Specifications;

public class JournalEntriesInPeriodForAccountSpec : Specification<JournalEntry>
{
    public JournalEntriesInPeriodForAccountSpec(Guid accountId, DateTime startDate, DateTime endDate)
    {
        Query
            .Where(je => je.Date >= startDate && je.Date <= endDate && je.Transactions.Any(t => t.AccountId == accountId))
            .Include(je => je.Transactions) // Include all transactions, handler will filter for the specific account
            .OrderBy(je => je.Date)         // Order by date
            .ThenBy(je => je.CreatedOn);    // Secondary sort for transactions on the same day for consistent running balance
                                            // Assuming JournalEntry.CreatedOn provides a consistent order if EntryNumber isn't sequential/sortable
    }
}
