using FSH.WebApi.Domain.Accounting.Enums;
using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class JournalEntry : AuditableEntity, IAggregateRoot
{
    public DateTime EntryDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public JournalEntryStatus Status { get; private set; }
    public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    public JournalEntry(DateTime entryDate, string description)
    {
        EntryDate = entryDate;
        Description = description;
        Status = JournalEntryStatus.Draft;
        // CreatedAt and UpdatedAt are handled by AuditableEntity base class
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void RemoveTransaction(Transaction transaction)
    {
        Transactions.Remove(transaction);
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void Post()
    {
        if (Status != JournalEntryStatus.Draft)
        {
            throw new InvalidOperationException("Only draft entries can be posted.");
        }

        // Add validation logic here to ensure debits equal credits
        if (!ValidateTransactions())
        {
            throw new InvalidOperationException("Debits must equal credits to post the journal entry.");
        }

        Status = JournalEntryStatus.Posted;
        foreach (var transaction in Transactions)
        {
            if (transaction.TransactionType == TransactionType.Debit)
            {
                transaction.Account.Debit(transaction.Amount);
            }
            else
            {
                transaction.Account.Credit(transaction.Amount);
            }
        }
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void Void()
    {
        if (Status != JournalEntryStatus.Posted)
        {
            throw new InvalidOperationException("Only posted entries can be voided.");
        }
        Status = JournalEntryStatus.Voided;
        // Add logic here to reverse the impact of the transactions if necessary
        foreach (var transaction in Transactions)
        {
            if (transaction.TransactionType == TransactionType.Debit)
            {
                // Reverse the debit by crediting
                transaction.Account.Credit(transaction.Amount);
            }
            else
            {
                // Reverse the credit by debiting
                transaction.Account.Debit(transaction.Amount);
            }
        }
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void UpdateJournalEntryDetails(DateTime entryDate, string description)
    {
        if (Status != JournalEntryStatus.Draft)
        {
            throw new InvalidOperationException("Only draft entries can be updated.");
        }
        EntryDate = entryDate;
        Description = description;
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    private bool ValidateTransactions()
    {
        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var transaction in Transactions)
        {
            if (transaction.TransactionType == TransactionType.Debit)
            {
                totalDebits += transaction.Amount;
            }
            else
            {
                totalCredits += transaction.Amount;
            }
        }

        return totalDebits == totalCredits;
    }
}
