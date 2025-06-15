using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class OutstandingTransactionsReportDto
{
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = default!; // Populated by handler
    public string BankAccountNumber { get; set; } = default!; // Populated by handler
    public DateTime AsOfDate { get; set; }
    public string TypeFilterApplied { get; set; } = default!; // String representation of the enum filter used
    public int LookbackWindowDaysUsed { get; set; } // Echo back the window days used

    public List<OutstandingTransactionLineDto> Transactions { get; set; } = new();

    public int TotalCount => Transactions.Count;
    public decimal TotalAmount { get; set; } // Sum of amounts for the listed transactions
    public decimal TotalUnclearedChecksOrDebitsAmount { get; set; } // Specific total if filtered
    public decimal TotalDepositsInTransitOrCreditsAmount { get; set; } // Specific total if filtered

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class OutstandingTransactionLineDto
{
    public Guid SystemTransactionId { get; set; } // This is the FSH.WebApi.Domain.Accounting.Transaction.Id
    public DateTime TransactionDate { get; set; } // This is the JournalEntry.Date
    public string TransactionType { get; set; } = default!; // "Uncleared Check/Debit" or "Deposit in Transit/Credit"
    public string? JournalEntryNumber { get; set; } // From JournalEntry.EntryNumber (or Id)
    public string? Reference { get; set; }           // From JournalEntry.Reference
    public string Description { get; set; } = default!; // From Transaction.Description or JournalEntry.Description
    public decimal Amount { get; set; }             // The amount of the transaction (always positive)
    public string AmountType { get; set; } = default!; // "Debit" or "Credit" (reflecting the GL transaction side)
    public int DaysOutstanding { get; set; }        // (AsOfDate - TransactionDate).Days

    // Optional: Could add more details if needed, like Payee from JE or other related info.
    // public string? PayeeOrSource { get; set; }

    public OutstandingTransactionLineDto() { }
}
