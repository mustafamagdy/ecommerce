using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class BankAccountDto : IDto
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string BankName { get; set; } = default!;
    public string? BranchName { get; set; }
    public string Currency { get; set; } = default!;
    public Guid GLAccountId { get; set; }
    public string? GLAccountCode { get; set; } // For display (e.g., from Account.AccountNumber)
    public string? GLAccountName { get; set; } // For display (e.g., from Account.AccountName)
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
