using FSH.WebApi.Domain.Accounting.Enums;
using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class Account : AuditableEntity, IAggregateRoot
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }

    public Account(string accountNumber, string accountName, AccountType accountType, decimal balance = 0)
    {
        AccountNumber = accountNumber;
        AccountName = accountName;
        AccountType = accountType;
        Balance = balance; // Renamed initialBalance to balance
        IsActive = true;
        // CreatedAt and UpdatedAt are handled by AuditableEntity base class
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be positive.");
        }

        switch (AccountType)
        {
            case AccountType.Asset:
            case AccountType.Expense:
                Balance += amount;
                break;
            case AccountType.Liability:
            case AccountType.Equity:
            case AccountType.Revenue:
                Balance -= amount;
                break;
        }
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be positive.");
        }

        switch (AccountType)
        {
            case AccountType.Asset:
            case AccountType.Expense:
                Balance -= amount;
                break;
            case AccountType.Liability:
            case AccountType.Equity:
            case AccountType.Revenue:
                Balance += amount;
                break;
        }
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void Activate()
    {
        IsActive = true;
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void Deactivate()
    {
        IsActive = false;
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }

    public void UpdateAccountDetails(string accountNumber, string accountName, AccountType accountType)
    {
        AccountNumber = accountNumber;
        AccountName = accountName;
        AccountType = accountType;
        // UpdatedAt is handled by AuditableEntity base class or DbContext
    }
}
