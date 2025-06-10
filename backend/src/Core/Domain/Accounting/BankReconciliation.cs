using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic; // For potential future use with matches
using System.Linq; // For potential future use with matches

namespace FSH.WebApi.Domain.Accounting;

public enum ReconciliationStatus
{
    Draft,
    InProgress, // When user is actively matching transactions
    PendingReview, // Optional step
    Completed,
    Closed // Finalized, no more changes allowed
}

public class BankReconciliation : AuditableEntity, IAggregateRoot
{
    public Guid BankAccountId { get; private set; }
    // public virtual BankAccount BankAccount { get; private set; } = default!; // Navigation

    public DateTime ReconciliationDate { get; private set; } // End date of the period being reconciled
    public Guid BankStatementId { get; private set; } // Link to the bank statement used
    // public virtual BankStatement BankStatement { get; private set; } = default!; // Navigation

    public decimal StatementBalance { get; private set; } // Closing balance from the bank statement
    public decimal SystemBalance { get; private set; } // Calculated GL balance for the bank account as of ReconciliationDate
    public decimal Difference => StatementBalance - SystemBalance - CalculatedUnclearedChecks() + CalculatedDepositsInTransit(); // Simplified for now
                                                                                                                              // A more accurate difference might also account for outstanding checks and deposits in transit.
                                                                                                                              // This calculation can get complex and might be a snapshot value updated by a process.
    public ReconciliationStatus Status { get; private set; }

    // Sum of reconciled BankStatementTransactions that are DEBITS (checks, withdrawals) not yet cleared in GL by statement date.
    // This is a simplification. True "uncleared checks" are system-side transactions not yet on bank statement.
    // And "deposits in transit" are system-side deposits not yet on bank statement.
    // For now, these might be manual inputs or calculated by a more complex process.
    // Let's assume these are values that might be determined during the reconciliation process.
    public decimal ManuallyAssignedUnclearedChecks { get; private set; } // Example property
    public decimal ManuallyAssignedDepositsInTransit { get; private set; } // Example property


    // Private constructor for EF Core
    private BankReconciliation() { }

    public BankReconciliation(
        Guid bankAccountId,
        DateTime reconciliationDate,
        Guid bankStatementId,
        decimal statementBalance,
        decimal systemBalance, // Initial system balance from GL
        ReconciliationStatus status = ReconciliationStatus.Draft)
    {
        BankAccountId = bankAccountId;
        ReconciliationDate = reconciliationDate;
        BankStatementId = bankStatementId;
        StatementBalance = statementBalance;
        SystemBalance = systemBalance; // This would come from querying the GL Account balance
        Status = status;
        ManuallyAssignedUnclearedChecks = 0;
        ManuallyAssignedDepositsInTransit = 0;
    }

    // This is a simplified method for calculating difference based on what's directly on the statement.
    // Real reconciliation matches bank transactions to system transactions.
    // The "Difference" should ideally be zero after all matching and adjustments.
    private decimal CalculatedUnclearedChecks() => ManuallyAssignedUnclearedChecks; // Placeholder for actual calculation
    private decimal CalculatedDepositsInTransit() => ManuallyAssignedDepositsInTransit; // Placeholder


    public void UpdateBalancesAndStatus(decimal? statementBalance, decimal? systemBalance, ReconciliationStatus? status, decimal? unclearedChecks = null, decimal? depositsInTransit = null)
    {
        if (Status == ReconciliationStatus.Completed || Status == ReconciliationStatus.Closed)
            throw new InvalidOperationException($"Cannot update reconciliation in '{Status}' status.");

        if (statementBalance.HasValue) StatementBalance = statementBalance.Value;
        if (systemBalance.HasValue) SystemBalance = systemBalance.Value;
        if (status.HasValue) Status = status.Value;
        if (unclearedChecks.HasValue) ManuallyAssignedUnclearedChecks = unclearedChecks.Value;
        if (depositsInTransit.HasValue) ManuallyAssignedDepositsInTransit = depositsInTransit.Value;
    }

    public void CompleteReconciliation()
    {
        // Add validation: Difference must be zero (or within tolerance) before completing.
        // For now, allowing completion regardless of difference for simplicity.
        // if (Difference != 0)
        //    throw new InvalidOperationException("Reconciliation difference is not zero. Cannot complete.");

        Status = ReconciliationStatus.Completed;
    }

    public void CloseReconciliation()
    {
        if (Status != ReconciliationStatus.Completed)
            throw new InvalidOperationException("Reconciliation must be completed before it can be closed.");
        Status = ReconciliationStatus.Closed;
    }
}
