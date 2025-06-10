using FSH.WebApi.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class CustomerInvoiceDto : IDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; } // For display
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; } // For display, if linked to an order
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue => TotalAmount - AmountPaid; // Calculated property
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!; // Mapped from CustomerInvoiceStatus enum
    public string? Notes { get; set; }
    public List<CustomerInvoiceItemDto> InvoiceItems { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public class CustomerInvoiceItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid CustomerInvoiceId { get; set; } // May not be needed if always child
    public string Description { get; set; } = default!;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; } // For display
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; } // Line total (Quantity * UnitPrice)
    public decimal TaxAmount { get; set; }
}
