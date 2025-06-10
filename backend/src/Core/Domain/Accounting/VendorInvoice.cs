using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Domain.Accounting;

public enum VendorInvoiceStatus
{
    Draft,
    Submitted,
    Approved,
    Paid,
    Cancelled // Added Cancelled as a common status
}

public class VendorInvoice : AuditableEntity, IAggregateRoot
{
    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; } = default!; // Navigation property
    public string InvoiceNumber { get; private set; } = default!;
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = default!; // Consider using a dedicated Currency entity or enum if needed
    public VendorInvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; } // Added for any additional notes

    private readonly List<VendorInvoiceItem> _invoiceItems = new();
    public IReadOnlyCollection<VendorInvoiceItem> InvoiceItems => _invoiceItems.AsReadOnly();

    public VendorInvoice(
        Guid supplierId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime dueDate,
        decimal totalAmount,
        string currency,
        VendorInvoiceStatus status,
        string? notes = null)
    {
        SupplierId = supplierId;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        TotalAmount = totalAmount;
        Currency = currency;
        Status = status;
        Notes = notes;
    }

    public VendorInvoice Update(
        string? invoiceNumber,
        DateTime? invoiceDate,
        DateTime? dueDate,
        decimal? totalAmount,
        string? currency,
        VendorInvoiceStatus? status,
        string? notes)
    {
        if (invoiceNumber is not null && InvoiceNumber?.Equals(invoiceNumber) is not true) InvoiceNumber = invoiceNumber;
        if (invoiceDate.HasValue && InvoiceDate != invoiceDate.Value) InvoiceDate = invoiceDate.Value;
        if (dueDate.HasValue && DueDate != dueDate.Value) DueDate = dueDate.Value;
        if (totalAmount.HasValue && TotalAmount != totalAmount.Value) TotalAmount = totalAmount.Value;
        if (currency is not null && Currency?.Equals(currency) is not true) Currency = currency;
        if (status.HasValue && Status != status.Value) Status = status.Value;
        if (notes is not null && Notes?.Equals(notes) is not true) Notes = notes;
        return this;
    }

    public void AddInvoiceItem(VendorInvoiceItem item)
    {
        // Consider validation: e.g., item should not be null, item should belong to this invoice
        _invoiceItems.Add(item);
    }

    public void RemoveInvoiceItem(Guid vendorInvoiceItemId)
    {
        var item = _invoiceItems.FirstOrDefault(i => i.Id == vendorInvoiceItemId);
        if (item != null)
        {
            _invoiceItems.Remove(item);
        }
    }

    public void ClearInvoiceItems()
    {
        _invoiceItems.Clear();
    }

    // Method to update status, could have more complex logic
    public void UpdateStatus(VendorInvoiceStatus newStatus)
    {
        // Add any validation or business logic for status transitions here
        // For example, an invoice cannot be marked as Paid if it's still in Draft.
        Status = newStatus;
    }
}
