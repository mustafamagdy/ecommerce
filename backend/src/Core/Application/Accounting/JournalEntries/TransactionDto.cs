using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class TransactionDto : IDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; } // Optional: for display purposes
    public string TransactionType { get; set; } = default!; // "Debit" or "Credit"
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
