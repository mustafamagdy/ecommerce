using FSH.WebApi.Domain.Accounting.Enums;
using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class Transaction : AuditableEntity
{
    public Guid AccountId { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }

    // Navigation properties
    public virtual Account Account { get; private set; } = default!;
    public virtual JournalEntry JournalEntry { get; private set; } = default!;

    public Transaction(Guid accountId, Guid journalEntryId, TransactionType transactionType, decimal amount, string description, DateTime transactionDate)
    {
        AccountId = accountId;
        JournalEntryId = journalEntryId;
        TransactionType = transactionType;
        Amount = amount;
        Description = description;
        TransactionDate = transactionDate;
        // CreatedAt and UpdatedAt are handled by AuditableEntity base class
    }

    public void UpdateTransactionDetails(TransactionType transactionType, decimal amount, string description, DateTime transactionDate)
    {
        TransactionType = transactionType;
        Amount = amount;
        Description = description;
        TransactionDate = transactionDate;
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }
}
