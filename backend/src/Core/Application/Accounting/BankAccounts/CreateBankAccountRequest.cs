using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class CreateBankAccountRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = default!;

    [Required]
    [MaxLength(50)] // Max length for account numbers can vary greatly
    public string AccountNumber { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string BankName { get; set; } = default!;

    [MaxLength(100)]
    public string? BranchName { get; set; }

    [Required]
    [MaxLength(3)] // E.g., "USD", "EUR"
    public string Currency { get; set; } = default!;

    [Required]
    public Guid GLAccountId { get; set; } // Link to an existing GL Account

    public bool IsActive { get; set; } = true;
}
