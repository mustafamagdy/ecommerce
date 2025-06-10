using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For CustomerInvoiceStatus if status update is allowed here

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class UpdateCustomerInvoiceRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; } // ID of the CustomerInvoice to update

    // Typically CustomerId and OrderId might not be updatable once an invoice is created.
    // public Guid? CustomerId { get; set; }
    // public Guid? OrderId { get; set; }

    // InvoiceNumber is usually not changed.
    // public string? InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Status updates might be better handled by dedicated requests (e.g., SendInvoiceRequest, MarkAsPaidRequest)
    // public CustomerInvoiceStatus? Status { get; set; }

    // For items, the list could represent the desired final state.
    // The handler would need to diff this list with existing items.
    public List<UpdateCustomerInvoiceItemRequest>? InvoiceItems { get; set; }
}

public class UpdateCustomerInvoiceItemRequest
{
    public Guid? Id { get; set; } // Null or Guid.Empty for new items, existing Id for items to update

    [MaxLength(1000)]
    public string? Description { get; set; } // Required if new, optional if updating

    public Guid? ProductId { get; set; }

    [Range(0.0001, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TaxAmount { get; set; }
}
