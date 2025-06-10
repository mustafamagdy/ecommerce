using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class CreatePaymentMethodRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
