using FSH.WebApi.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class JournalEntryDto : IDto
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = default!;
    public string? ReferenceNumber { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
}
