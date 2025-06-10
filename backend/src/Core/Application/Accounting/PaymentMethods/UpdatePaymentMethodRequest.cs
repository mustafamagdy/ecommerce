using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class UpdatePaymentMethodRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
