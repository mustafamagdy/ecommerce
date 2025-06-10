using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For BankTransactionType

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class CreateBankStatementRequest : IRequest<Guid>
{
    [Required]
    public Guid BankAccountId { get; set; }

    [Required]
    public DateTime StatementDate { get; set; }

    [Required]
    public decimal OpeningBalance { get; set; }

    [Required]
    public decimal ClosingBalance { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; } // Statement number from bank

    // ImportDate will be set by the handler/domain entity.

    [Required]
    [MinLength(1, ErrorMessage = "Bank statement must have at least one transaction or be an empty statement marker.")]
    // Allow empty list if it's a zero-transaction statement (e.g. new account, no activity)
    // Or enforce MinLength(1) if that's not allowed. For now, allowing empty.
    public List<CreateBankStatementTransactionRequestItem> Transactions { get; set; } = new();
}

public class CreateBankStatementTransactionRequestItem
{
    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = default!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public BankTransactionType Type { get; set; } // Debit or Credit

    [MaxLength(100)]
    public string? Reference { get; set; } // E.g., Check number

    [MaxLength(100)]
    public string? BankProvidedId { get; set; } // Unique ID from bank for this transaction
}
