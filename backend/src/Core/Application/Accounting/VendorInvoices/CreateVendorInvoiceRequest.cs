using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For VendorInvoiceStatus, though status is usually set by backend logic

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class CreateVendorInvoiceRequest : IRequest<Guid>
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public string InvoiceNumber { get; set; } = default!;

    [Required]
    public DateTime InvoiceDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    // TotalAmount will be calculated from items, but can be sent for validation
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(3)] // e.g., "USD", "EUR"
    public string Currency { get; set; } = default!;

    // Status is typically not set by client on creation, defaults to Draft or Submitted.
    // public VendorInvoiceStatus InitialStatus { get; set; } = VendorInvoiceStatus.Draft;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateVendorInvoiceItemRequest> InvoiceItems { get; set; } = new();
}

public class CreateVendorInvoiceItemRequest
{
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = default!;

    public Guid? ProductId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue)] // Quantity could be fractional for services/materials
    public decimal Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    // TotalAmount for item is Quantity * UnitPrice, can be validated
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }


    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; } = 0;
}
