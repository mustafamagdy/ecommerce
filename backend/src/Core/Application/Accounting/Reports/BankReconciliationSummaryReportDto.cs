using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class BankReconciliationSummaryReportDto
{
    public Guid BankReconciliationId { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = default!;    // From BankAccount.AccountName
    public string BankAccountNumber { get; set; } = default!; // From BankAccount.AccountNumber
    public string BankAccountCurrency { get; set; } = default!; // From BankAccount.Currency
    public DateTime ReconciliationDate { get; set; }            // From BankReconciliation.ReconciliationDate
    public Guid BankStatementId { get; set; }
    public DateTime StatementDate { get; set; }                 // From linked BankStatement.StatementDate
    public string? BankStatementReference { get; set; }          // From linked BankStatement.ReferenceNumber

    // Balances from the BankReconciliation entity
    public decimal StatementEndingBalance { get; set; }         // BankReconciliation.StatementBalance
    public decimal SystemGlBalanceAsPerRecon { get; set; }    // BankReconciliation.SystemBalance (as recorded in the recon)

    // Reconciling items - these would typically be calculated by the handler
    // based on unmatched items from both bank statement and system transactions (GL).
    // The BankReconciliation domain entity has ManuallyAssignedUnclearedChecks and ManuallyAssignedDepositsInTransit.
    // For this DTO, we will reflect those, and the handler can expand on this logic.
    public decimal UnclearedChecksOrDebitsAmount { get; set; }   // Sum of system payments/debits not on bank statement (or BankReconciliation.ManuallyAssignedUnclearedChecks)
    public int UnclearedChecksOrDebitsCount { get; set; }
    public List<BankTransactionDetailDto>? UnclearedChecksOrDebitsDetails { get; set; } // Optional detailed list

    public decimal DepositsInTransitOrCreditsAmount { get; set; } // Sum of system deposits/credits not on bank statement (or BankReconciliation.ManuallyAssignedDepositsInTransit)
    public int DepositsInTransitOrCreditsCount { get; set; }
    public List<BankTransactionDetailDto>? DepositsInTransitOrCreditsDetails { get; set; } // Optional detailed list

    // Other adjustments could be items identified during reconciliation that need booking to either side.
    // For now, these are simplified.
    public decimal BankAdjustmentsNotOnSystemAmount { get; set; } // e.g. Bank fees, interest income shown on statement but not yet in GL
    public int BankAdjustmentsNotOnSystemCount { get; set; }
    public List<BankTransactionDetailDto>? BankAdjustmentsNotOnSystemDetails { get; set; }


    // Calculated Balances for the report
    // Balance per Bank Statement: StatementEndingBalance
    // Add: Deposits in Transit (system deposits not on statement)
    // Less: Outstanding Checks (system payments not on statement)
    // = Adjusted Bank Balance (or "True" Cash Balance from Bank's perspective after timing differences)
    public decimal CalculatedAdjustedBankBalance { get; set; }

    // Balance per General Ledger: SystemGlBalance (as of recon date)
    // Add: Bank Interest / Other Credits not yet booked in GL
    // Less: Bank Charges / Other Debits not yet booked in GL
    // = Adjusted GL Balance (or "True" Cash Balance from Book's perspective after timing differences)
    public decimal CalculatedAdjustedGlBalance { get; set; }


    // This is the unexplained difference from the BankReconciliation entity. Ideally should be zero.
    public decimal UnexplainedDifference { get; set; } // From BankReconciliation.Difference

    public string Status { get; set; } = default!; // From BankReconciliation.Status enum
    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

// Simplified DTO for listing individual transactions that form reconciling items
public class BankTransactionDetailDto
{
    public Guid OriginalTransactionId { get; set; } // ID of the BankStatementTransaction or System-Side Transaction (e.g. Payment ID)
    public string TransactionSource { get; set; } = default!; // "Bank Statement" or "General Ledger" or specific module like "AP Payment"
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Type { get; set; } = default!; // "Debit" or "Credit" (from the perspective of the source, e.g. Bank Debit)

    public BankTransactionDetailDto() { }
}
