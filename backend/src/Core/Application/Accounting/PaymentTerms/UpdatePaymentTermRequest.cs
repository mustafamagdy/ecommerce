using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class UpdatePaymentTermRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; } // Optional for update, but if provided, validated

    [MaxLength(256)]
    public string? Description { get; set; }

    [Range(0, 3650)]
    public int? DaysUntilDue { get; set; }

    public bool? IsActive { get; set; }
}
