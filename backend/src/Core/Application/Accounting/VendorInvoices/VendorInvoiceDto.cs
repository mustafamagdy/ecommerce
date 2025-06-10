using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Accounting; // For VendorInvoiceStatus enum
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class VendorInvoiceDto : IDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; } // For display purposes
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!; // Mapped from VendorInvoiceStatus enum
    public string? Notes { get; set; }
    public List<VendorInvoiceItemDto> InvoiceItems { get; set; } = new();
}

public class VendorInvoiceItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid VendorInvoiceId { get; set; } // May not be needed in DTO if always child of VendorInvoiceDto
    public string Description { get; set; } = default!;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; } // For display purposes
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
}
