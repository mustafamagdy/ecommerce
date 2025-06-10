using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Domain.Accounting;

public class BankStatement : AuditableEntity, IAggregateRoot
{
    public Guid BankAccountId { get; private set; }
    // public virtual BankAccount BankAccount { get; private set; } = default!; // Navigation

    public DateTime StatementDate { get; private set; } // Usually the end date of the statement period
    public decimal OpeningBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public string? ReferenceNumber { get; private set; } // E.g., statement number from the bank
    public DateTime ImportDate { get; private set; } // When this statement was imported/entered

    private readonly List<BankStatementTransaction> _transactions = new();
    public IReadOnlyCollection<BankStatementTransaction> Transactions => _transactions.AsReadOnly();

    // Private constructor for EF Core
    private BankStatement() { }

    public BankStatement(
        Guid bankAccountId,
        DateTime statementDate,
        decimal openingBalance,
        decimal closingBalance,
        string? referenceNumber = null)
    {
        BankAccountId = bankAccountId;
        StatementDate = statementDate;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
        ReferenceNumber = referenceNumber;
        ImportDate = DateTime.UtcNow; // Defaults to now
    }

    public void AddTransaction(BankStatementTransaction transaction)
    {
        if (transaction.BankStatementId != Guid.Empty && transaction.BankStatementId != this.Id)
            throw new ArgumentException("Transaction belongs to another statement.", nameof(transaction));

        // Additional validation: ensure transaction date is within statement period (approx) could be added.
        // Ensure sum of transactions reconciles with opening/closing balances (this is complex, usually done during reconciliation process itself)

        _transactions.Add(transaction);
    }

    public void AddTransactions(IEnumerable<BankStatementTransaction> transactions)
    {
        foreach (var tx in transactions)
        {
            AddTransaction(tx);
        }
    }

    public BankStatement Update(DateTime? statementDate, decimal? openingBalance, decimal? closingBalance, string? referenceNumber)
    {
        // Generally, once a statement is imported and has transactions, it might be restricted from updates,
        // or updates might trigger re-validation of transactions and reconciliations.
        // For simplicity, allowing updates for now.

        if (statementDate.HasValue) StatementDate = statementDate.Value;
        if (openingBalance.HasValue) OpeningBalance = openingBalance.Value;
        if (closingBalance.HasValue) ClosingBalance = closingBalance.Value;
        if (referenceNumber is not null) ReferenceNumber = referenceNumber;

        return this;
    }

    public void SetImportDate(DateTime importDate)
    {
        ImportDate = importDate;
    }
}
