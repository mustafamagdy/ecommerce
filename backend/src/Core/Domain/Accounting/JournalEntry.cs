using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Domain.Accounting;

public class JournalEntry : AuditableEntity, IAggregateRoot
{
    public DateTime EntryDate { get; private set; }
    public string Description { get; private set; } = default!;
    public string? ReferenceNumber { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public bool IsPosted { get; private set; } = false;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public JournalEntry(DateTime entryDate, string description, string? referenceNumber)
    {
        EntryDate = entryDate;
        Description = description;
        ReferenceNumber = referenceNumber;
    }

    public JournalEntry Update(DateTime? entryDate, string? description, string? referenceNumber, DateTime? postedDate, bool? isPosted)
    {
        if (entryDate.HasValue && EntryDate != entryDate.Value) EntryDate = entryDate.Value;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (referenceNumber is not null && ReferenceNumber?.Equals(referenceNumber) is not true) ReferenceNumber = referenceNumber;
        if (postedDate.HasValue && PostedDate != postedDate.Value) PostedDate = postedDate.Value;
        if (isPosted.HasValue && IsPosted != isPosted.Value) IsPosted = isPosted.Value;
        return this;
    }

    public void Post()
    {
        IsPosted = true;
        PostedDate = DateTime.UtcNow;
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }
}
