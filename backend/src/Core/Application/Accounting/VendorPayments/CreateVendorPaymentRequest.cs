using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class CreateVendorPaymentRequest : IRequest<Guid>
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount Paid must be greater than zero.")]
    public decimal AmountPaid { get; set; }

    [Required]
    public Guid PaymentMethodId { get; set; }

    [MaxLength(256)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one invoice application is required.")]
    public List<VendorPaymentApplicationRequestItem> Applications { get; set; } = new();
}

public class VendorPaymentApplicationRequestItem
{
    [Required]
    public Guid VendorInvoiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount Applied must be greater than zero.")]
    public decimal AmountApplied { get; set; }
}
