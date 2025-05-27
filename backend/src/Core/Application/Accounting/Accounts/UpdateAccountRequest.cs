using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class UpdateAccountRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string? AccountName { get; set; }

    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    public string? AccountType { get; set; } // Will be validated against AccountType enum

    [MaxLength(1024)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
