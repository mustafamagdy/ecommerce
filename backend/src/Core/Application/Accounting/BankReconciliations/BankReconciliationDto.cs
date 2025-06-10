using FSH.WebApi.Application.Common.Interfaces;
using System;
// using FSH.WebApi.Application.Accounting.BankStatements; // Potentially for BankStatementTransactionDto if embedded

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class BankReconciliationDto : IDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string? BankAccountName { get; set; } // For display
    public DateTime ReconciliationDate { get; set; }
    public Guid BankStatementId { get; set; }
    public string? BankStatementReference { get; set; } // For display
    public decimal StatementBalance { get; set; }
    public decimal SystemBalance { get; set; } // GL balance
    public decimal Difference { get; set; }
    public string Status { get; set; } = default!; // Mapped from ReconciliationStatus enum
    public decimal ManuallyAssignedUnclearedChecks { get; set; }
    public decimal ManuallyAssignedDepositsInTransit { get; set; }

    // Optionally, could include lists of reconciled/unreconciled transactions for this period
    // List<BankStatementTransactionDto> MatchedTransactions { get; set; } = new();
    // List<BankStatementTransactionDto> UnmatchedBankTransactions { get; set; } = new();
    // List<SystemTransactionDto> UnmatchedSystemTransactions { get; set; } = new(); // SystemTransactionDto would need to be defined

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
