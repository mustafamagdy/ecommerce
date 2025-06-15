using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For JournalEntry, Transaction
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.JournalEntries.Specifications;

public class JournalEntriesForGLAccountInDateWindowSpec : Specification<JournalEntry>
{
    /// <summary>
    /// Fetches Journal Entries that have transactions affecting a specific GL Account,
    /// within a given date window (inclusive of start and end date of JE).
    /// Includes all transactions for those Journal Entries.
    /// </summary>
    /// <param name="glAccountId">The GL Account ID to filter transactions by.</param>
    /// <param name="windowStartDate">The start of the date window for Journal Entry dates.</param>
    /// <param name="windowEndDate">The end of the date window for Journal Entry dates.</param>
    public JournalEntriesForGLAccountInDateWindowSpec(Guid glAccountId, DateTime windowStartDate, DateTime windowEndDate)
    {
        // We need Journal Entries that *contain* transactions for the given glAccountId,
        // and the Journal Entry's date falls within the window.
        Query
            .Where(je => je.Date >= windowStartDate &&
                         je.Date <= windowEndDate && // Ensure JE date is within window
                         je.Transactions.Any(t => t.AccountId == glAccountId)) // Ensure at least one tx hits the account
            .Include(je => je.Transactions) // Include all transactions for these JEs
            .OrderBy(je => je.Date)
            .ThenBy(je => je.CreatedOn);
    }
}
