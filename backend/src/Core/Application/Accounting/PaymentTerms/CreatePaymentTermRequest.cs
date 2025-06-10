using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class CreatePaymentTermRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? Description { get; set; }

    [Required]
    [Range(0, 3650)] // Allowing 0 days, up to 10 years for flexibility
    public int DaysUntilDue { get; set; }

    public bool IsActive { get; set; } = true;
}
