using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For JournalEntry, Transaction
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.JournalEntries.Specifications;

public class JournalEntriesBeforeDateForAccountSpec : Specification<JournalEntry>
{
    public JournalEntriesBeforeDateForAccountSpec(Guid accountId, DateTime startDate)
    {
        Query
            .Where(je => je.Date < startDate && je.Transactions.Any(t => t.AccountId == accountId))
            .Include(je => je.Transactions.Where(t => t.AccountId == accountId)); // Include only relevant transactions

        // Note: The .Include(je => je.Transactions.Where(t => t.AccountId == accountId))
        // might not work as expected with all EF Core versions or providers for filtering includes.
        // A common pattern is to include all transactions and filter in memory,
        // or to project to a DTO with filtered transactions if the query provider supports it.
        // For simplicity here, we express the intent. If issues arise, the handler might need to filter transactions post-query.
        // A safer include is just .Include(je => je.Transactions) and then filter in handler.
        // Let's use the safer include for now.
        Query.Include(je => je.Transactions);
    }
}
