using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Operation.Customers; // Assuming this path is correct for Customer
using FSH.WebApi.Domain.Operation.Orders;   // Assuming this path is correct for Order
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Domain.Accounting;

public enum CustomerInvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Void,
    Overdue // Added Overdue as a common status
}

public class CustomerInvoice : AuditableEntity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }
    // public virtual Customer Customer { get; private set; } = default!; // Navigation if Customer is in the same DbContext & ORM scope

    public Guid? OrderId { get; private set; }
    // public virtual Order? Order { get; private set; } // Navigation if Order is in the same DbContext & ORM scope

    public string InvoiceNumber { get; private set; } = default!; // Should be unique
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; } // Calculated from items
    public decimal AmountPaid { get; private set; } // Total amount paid against this invoice
    public string Currency { get; private set; } = default!; // E.g., "USD", "EUR"
    public CustomerInvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<CustomerInvoiceItem> _invoiceItems = new();
    public IReadOnlyCollection<CustomerInvoiceItem> InvoiceItems => _invoiceItems.AsReadOnly();

    // Private constructor for EF Core
    private CustomerInvoice() { }

    public CustomerInvoice(
        Guid customerId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime dueDate,
        string currency,
        string? notes = null,
        Guid? orderId = null,
        CustomerInvoiceStatus status = CustomerInvoiceStatus.Draft)
    {
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        Currency = currency;
        Notes = notes;
        OrderId = orderId;
        Status = status;
        TotalAmount = 0; // Will be set by adding items
        AmountPaid = 0;
    }

    public void AddInvoiceItem(string description, decimal quantity, decimal unitPrice, decimal taxAmount, Guid? productId = null)
    {
        var item = new CustomerInvoiceItem(this.Id, description, quantity, unitPrice, taxAmount, productId);
        _invoiceItems.Add(item);
        RecalculateTotalAmount();
    }

    public void RemoveInvoiceItem(Guid customerInvoiceItemId)
    {
        var item = _invoiceItems.FirstOrDefault(i => i.Id == customerInvoiceItemId);
        if (item != null)
        {
            _invoiceItems.Remove(item);
            RecalculateTotalAmount();
        }
    }

    public void ClearInvoiceItems()
    {
        _invoiceItems.Clear();
        RecalculateTotalAmount();
    }

    private void RecalculateTotalAmount()
    {
        // TotalAmount should be the sum of (item line totals + item taxes)
        TotalAmount = _invoiceItems.Sum(item => item.TotalAmount + item.TaxAmount);
    }

    public void UpdateStatus(CustomerInvoiceStatus newStatus)
    {
        // Add validation or business logic for status transitions
        Status = newStatus;
    }

    public void ApplyPayment(decimal paymentAmount)
    {
        if (paymentAmount <= 0) throw new ArgumentOutOfRangeException(nameof(paymentAmount), "Payment amount must be positive.");

        AmountPaid += paymentAmount;

        if (AmountPaid >= TotalAmount)
        {
            Status = CustomerInvoiceStatus.Paid;
        }
        else if (AmountPaid > 0)
        {
            Status = CustomerInvoiceStatus.PartiallyPaid;
        }
        // Consider other status transitions, e.g., if it becomes overdue.
    }

    public decimal GetBalanceDue()
    {
        return TotalAmount - AmountPaid;
    }

    public void Update(
        DateTime? invoiceDate,
        DateTime? dueDate,
        string? currency,
        string? notes,
        CustomerInvoiceStatus? status)
    {
        if (invoiceDate.HasValue && InvoiceDate != invoiceDate.Value) InvoiceDate = invoiceDate.Value;
        if (dueDate.HasValue && DueDate != dueDate.Value) DueDate = dueDate.Value;
        if (!string.IsNullOrEmpty(currency) && Currency != currency) Currency = currency;
        if (notes is not null && Notes != notes) Notes = notes; // Allow setting notes to empty string
        if (status.HasValue && Status != status.Value) Status = status.Value; // Be careful with direct status updates
        // InvoiceNumber, CustomerId, OrderId are typically not updated.
        // TotalAmount is driven by items.
    }
}
