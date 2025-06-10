using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For VendorInvoiceStatus

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class UpdateVendorInvoiceRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; } // ID of the VendorInvoice to update

    public Guid? SupplierId { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? TotalAmount { get; set; } // Recalculated if items change

    [MaxLength(3)]
    public string? Currency { get; set; }

    // Status updates might be handled by separate dedicated requests (e.g., SubmitInvoiceRequest, ApproveInvoiceRequest)
    // public VendorInvoiceStatus? Status { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // For items, the list could represent the desired final state.
    // The handler would need to diff this list with existing items.
    // Items could be new (Id is null/empty), existing (Id is provided), or items not in list are to be removed.
    public List<UpdateVendorInvoiceItemRequest>? InvoiceItems { get; set; }
}

public class UpdateVendorInvoiceItemRequest
{
    public Guid? Id { get; set; } // Null or Guid.Empty for new items

    [MaxLength(1000)]
    public string? Description { get; set; } // Required if new, optional if updating

    public Guid? ProductId { get; set; } // Use Guid.Empty or some other convention to clear ProductId if needed

    [Range(0.0001, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    // TotalAmount for item is Quantity * UnitPrice
    [Range(0.01, double.MaxValue)]
    public decimal? TotalAmount { get; set; }


    [Range(0, double.MaxValue)]
    public decimal? TaxAmount { get; set; }
}
