using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class BankAccount : AuditableEntity, IAggregateRoot
{
    public string AccountName { get; private set; } = default!;
    public string AccountNumber { get; private set; } = default!; // E.g., IBAN or local account number
    public string BankName { get; private set; } = default!;
    public string? BranchName { get; private set; } // Optional
    public string Currency { get; private set; } = default!; // E.g., "USD", "EUR"
    public Guid GLAccountId { get; private set; } // Link to the main Account entity in GL
    // public virtual Account GLAccount { get; private set; } = default!; // Navigation property

    public bool IsActive { get; private set; } = true;

    // Private constructor for EF Core
    private BankAccount() { }

    public BankAccount(
        string accountName,
        string accountNumber,
        string bankName,
        string currency,
        Guid glAccountId,
        string? branchName = null,
        bool isActive = true)
    {
        AccountName = accountName;
        AccountNumber = accountNumber;
        BankName = bankName;
        Currency = currency;
        GLAccountId = glAccountId;
        BranchName = branchName;
        IsActive = isActive;
    }

    public BankAccount Update(
        string? accountName,
        string? accountNumber,
        string? bankName,
        string? currency,
        Guid? glAccountId,
        string? branchName,
        bool? isActive)
    {
        if (accountName is not null && AccountName?.Equals(accountName) is not true) AccountName = accountName;
        if (accountNumber is not null && AccountNumber?.Equals(accountNumber) is not true) AccountNumber = accountNumber;
        if (bankName is not null && BankName?.Equals(bankName) is not true) BankName = bankName;
        if (currency is not null && Currency?.Equals(currency) is not true) Currency = currency;
        if (glAccountId.HasValue && GLAccountId != glAccountId.Value) GLAccountId = glAccountId.Value;
        if (branchName is not null && BranchName?.Equals(branchName) is not true) BranchName = branchName;
        else if (string.IsNullOrEmpty(branchName) && BranchName is not null) BranchName = null; // Allow clearing
        if (isActive.HasValue && IsActive != isActive.Value) IsActive = isActive.Value;

        return this;
    }
}
