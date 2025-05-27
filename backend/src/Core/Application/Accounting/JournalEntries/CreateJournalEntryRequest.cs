using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class CreateJournalEntryRequest : IRequest<Guid>
{
    [Required]
    public DateTime EntryDate { get; set; }

    [Required]
    [MaxLength(1024)]
    public string Description { get; set; } = default!;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one transaction is required.")]
    public List<CreateTransactionRequestItem> Transactions { get; set; } = new();
}

public class CreateTransactionRequestItem
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public string TransactionType { get; set; } = default!; // "Debit" or "Credit", validated by FluentValidation

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }
}
