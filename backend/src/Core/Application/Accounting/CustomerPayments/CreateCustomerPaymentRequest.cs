using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CreateCustomerPaymentRequest : IRequest<Guid>
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount Received must be greater than zero.")]
    public decimal AmountReceived { get; set; }

    [Required]
    public Guid PaymentMethodId { get; set; }

    [MaxLength(256)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    // If empty, it's an unapplied payment. If provided, sum of AmountApplied should match AmountReceived.
    public List<CustomerPaymentApplicationRequestItem> Applications { get; set; } = new();
}

public class CustomerPaymentApplicationRequestItem
{
    [Required]
    public Guid CustomerInvoiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount Applied must be greater than zero.")]
    public decimal AmountApplied { get; set; }
}
