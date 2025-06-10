using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class UpdateBankAccountRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? AccountName { get; set; }

    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(100)]
    public string? BranchName { get; set; } // Pass empty string or null to clear

    [MaxLength(3)]
    public string? Currency { get; set; }

    public Guid? GLAccountId { get; set; }

    public bool? IsActive { get; set; }
}
