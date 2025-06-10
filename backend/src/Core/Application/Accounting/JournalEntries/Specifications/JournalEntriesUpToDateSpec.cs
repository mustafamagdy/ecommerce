using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For JournalEntry
using System;
using System.Linq; // Required for .ThenInclude()

namespace FSH.WebApi.Application.Accounting.JournalEntries.Specifications;

public class JournalEntriesUpToDateSpec : Specification<JournalEntry>
{
    public JournalEntriesUpToDateSpec(DateTime endDate)
    {
        Query
            .Where(je => je.Date <= endDate) // Filter by date
            .Include(je => je.Transactions)     // Ensure Transactions are loaded
            .OrderBy(je => je.Date);            // Optional: order by date for processing consistency
    }
}
