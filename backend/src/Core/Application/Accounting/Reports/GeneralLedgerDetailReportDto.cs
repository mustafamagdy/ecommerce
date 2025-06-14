using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class GeneralLedgerDetailReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid AccountId { get; set; } // Changed from Guid? to Guid to match request
    public string AccountCode { get; set; } = default!; // Populated from Account entity
    public string AccountName { get; set; } = default!; // Populated from Account entity
    public string AccountType { get; set; } = default!; // Populated from Account entity

    public decimal OpeningBalance { get; set; } // Opening balance for the account at StartDate (or before StartDate)
    public decimal ClosingBalance { get; set; } // Closing balance for the account at EndDate

    public List<GeneralLedgerDetailReportLineDto> Lines { get; set; } = new();

    public decimal TotalDebitsInPeriod { get; set; }  // Sum of debits within the period for this account
    public decimal TotalCreditsInPeriod { get; set; } // Sum of credits within the period for this account
    public decimal NetChangeInPeriod => TotalDebitsInPeriod - TotalCreditsInPeriod;

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class GeneralLedgerDetailReportLineDto
{
    public DateTime Date { get; set; } // Journal Entry Date
    public Guid JournalEntryId { get; set; }
    public string JournalEntryNumber { get; set; } = default!; // From JournalEntry (assuming it has one)
    public Guid TransactionId { get; set; } // Added ID of the specific transaction line

    public string? Description { get; set; } // From JournalEntry's Description or Transaction's Description
    public string? Reference { get; set; }   // From JournalEntry's Reference
    // public string? TransactionSource { get; set; } // E.g., "AP Payment", "AR Receipt", "Manual Journal" - could be derived

    public decimal DebitAmount { get; set; }  // Amount debited to this specific account
    public decimal CreditAmount { get; set; } // Amount credited to this specific account
    public decimal RunningBalance { get; set; } // Balance of the account after this transaction
}
