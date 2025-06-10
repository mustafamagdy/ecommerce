using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public enum BankTransactionType
{
    Debit,  // Money out from the bank's perspective (a charge on your account)
    Credit  // Money in from the bank's perspective (a deposit to your account)
}

public class BankStatementTransaction : AuditableEntity // Not an AggregateRoot, part of BankStatement aggregate
{
    public Guid BankStatementId { get; private set; }
    // public virtual BankStatement BankStatement { get; private set; } = default!; // Navigation

    public DateTime TransactionDate { get; private set; } // As it appears on the bank statement
    public string Description { get; private set; } = default!; // From bank statement
    public decimal Amount { get; private set; } // Absolute value, Type indicates direction
    public BankTransactionType Type { get; private set; } // Debit or Credit
    public string? Reference { get; private set; } // E.g., Check number, external transaction ID from bank
    public string? BankProvidedId { get; private set; } // Unique ID from the bank for this transaction, if available

    // Reconciliation fields
    public bool IsReconciled { get; private set; } = false;
    public Guid? SystemTransactionId { get; private set; } // Link to e.g., VendorPayment, CustomerPayment, JournalEntry
    public string? SystemTransactionType { get; private set; } // E.g., "VendorPayment", "CustomerPayment", "JournalEntry"
    public Guid? BankReconciliationId { get; private set; } // Link to the reconciliation run that reconciled this

    // Private constructor for EF Core
    private BankStatementTransaction() { }

    public BankStatementTransaction(
        Guid bankStatementId,
        DateTime transactionDate,
        string description,
        decimal amount,
        BankTransactionType type,
        string? reference = null,
        string? bankProvidedId = null)
    {
        BankStatementId = bankStatementId;
        TransactionDate = transactionDate;
        Description = description;
        Amount = amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive. Type (Debit/Credit) indicates direction.");
        Type = type;
        Reference = reference;
        BankProvidedId = bankProvidedId;
        IsReconciled = false; // Default to not reconciled
    }

    public void MarkAsReconciled(Guid reconciliationId, Guid? systemTransactionId, string? systemTransactionType)
    {
        if (IsReconciled && BankReconciliationId.HasValue && BankReconciliationId != reconciliationId)
        {
            // This implies it was reconciled by a different reconciliation run.
            // Business rule needed: Allow override? Throw error? Log warning?
            // For now, let's assume it can be re-reconciled by a new process if needed.
        }
        IsReconciled = true;
        SystemTransactionId = systemTransactionId;
        SystemTransactionType = systemTransactionType;
        BankReconciliationId = reconciliationId;
    }

    public void UnmarkAsReconciled()
    {
        IsReconciled = false;
        SystemTransactionId = null;
        SystemTransactionType = null;
        BankReconciliationId = null; // Remove link to the reconciliation run
    }

    // Update method for basic details, if allowed post-creation (usually statement transactions are immutable)
    public BankStatementTransaction UpdateDetails(string? description, string? reference, string? bankProvidedId)
    {
        if (description is not null && Description != description) Description = description;
        if (reference is not null && Reference != reference) Reference = reference;
        if (bankProvidedId is not null && BankProvidedId != bankProvidedId) BankProvidedId = bankProvidedId;
        return this;
    }
}
