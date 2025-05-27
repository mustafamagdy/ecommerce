using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Accounting;
using System;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class AccountDto : IDto
{
    public Guid Id { get; set; }
    public string AccountName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string AccountType { get; set; } = default!; // Mapped from enum
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
