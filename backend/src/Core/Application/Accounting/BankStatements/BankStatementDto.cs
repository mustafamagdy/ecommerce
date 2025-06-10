using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Accounting; // For BankTransactionType enum
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class BankStatementDto : IDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public string? BankAccountName { get; set; } // For display (e.g., "BankName - AccountNumber")
    public DateTime StatementDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime ImportDate { get; set; }
    public List<BankStatementTransactionDto> Transactions { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public class BankStatementTransactionDto : IDto
{
    public Guid Id { get; set; }
    // BankStatementId not usually needed in DTO if it's always a child
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Type { get; set; } = default!; // Mapped from BankTransactionType enum (Debit/Credit)
    public string? Reference { get; set; }
    public string? BankProvidedId { get; set; }

    public bool IsReconciled { get; set; }
    public Guid? SystemTransactionId { get; set; }
    public string? SystemTransactionType { get; set; }
    public string? SystemTransactionDetails { get; set; } // e.g. "Payment to X", "Payment from Y"
    public Guid? BankReconciliationId { get; set; }
    public DateTime CreatedOn { get; set; }
}
