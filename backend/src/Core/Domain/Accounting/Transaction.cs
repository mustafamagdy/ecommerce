using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class Transaction : AuditableEntity, IAggregateRoot
{
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public virtual JournalEntry JournalEntry { get; set; } = default!;
    public virtual Account Account { get; set; } = default!;

    public Transaction(Guid journalEntryId, Guid accountId, TransactionType transactionType, decimal amount, string? description)
    {
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        TransactionType = transactionType;
        Amount = amount;
        Description = description;
    }

    public Transaction Update(TransactionType? transactionType, decimal? amount, string? description)
    {
        if (transactionType.HasValue && TransactionType != transactionType.Value) TransactionType = transactionType.Value;
        if (amount.HasValue && Amount != amount.Value) Amount = amount.Value;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        return this;
    }
}
