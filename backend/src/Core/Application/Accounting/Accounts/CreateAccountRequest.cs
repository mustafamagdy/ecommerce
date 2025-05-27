using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class CreateAccountRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(256)]
    public string AccountName { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = default!;

    [Required]
    public string AccountType { get; set; } = default!; // Will be validated against AccountType enum

    public decimal InitialBalance { get; set; } = 0;

    [MaxLength(1024)]
    public string? Description { get; set; }
}
