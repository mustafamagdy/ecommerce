using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class Account : AuditableEntity, IAggregateRoot
{
    public string AccountName { get; private set; } = default!;
    public string AccountNumber { get; private set; } = default!;
    public AccountType AccountType { get; private set; }
    public decimal Balance { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Account(string accountName, string accountNumber, AccountType accountType, decimal balance, string? description, bool isActive = true)
    {
        AccountName = accountName;
        AccountNumber = accountNumber;
        AccountType = accountType;
        Balance = balance;
        Description = description;
        IsActive = isActive;
    }

    public Account Update(string? accountName, string? accountNumber, AccountType? accountType, decimal? balance, string? description, bool? isActive)
    {
        if (accountName is not null && AccountName?.Equals(accountName) is not true) AccountName = accountName;
        if (accountNumber is not null && AccountNumber?.Equals(accountNumber) is not true) AccountNumber = accountNumber;
        if (accountType.HasValue && AccountType != accountType.Value) AccountType = accountType.Value;
        if (balance.HasValue && Balance != balance.Value) Balance = balance.Value;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (isActive.HasValue && IsActive != isActive.Value) IsActive = isActive.Value;
        return this;
    }
}
