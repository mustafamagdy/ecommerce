using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Domain.Accounting;

public class VendorPayment : AuditableEntity, IAggregateRoot
{
    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; } = default!; // Navigation property

    public DateTime PaymentDate { get; private set; }
    public decimal AmountPaid { get; private set; }
    public Guid PaymentMethodId { get; private set; } // Link to a PaymentMethod entity (to be created or assumed existing)
    // public virtual PaymentMethod PaymentMethod { get; private set; } = default!; // Navigation property for PaymentMethod
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; } // Optional notes for the payment

    private readonly List<VendorPaymentApplication> _appliedInvoices = new();
    public IReadOnlyCollection<VendorPaymentApplication> AppliedInvoices => _appliedInvoices.AsReadOnly();

    public VendorPayment(
        Guid supplierId,
        DateTime paymentDate,
        decimal amountPaid,
        Guid paymentMethodId,
        string? referenceNumber = null,
        string? notes = null)
    {
        SupplierId = supplierId;
        PaymentDate = paymentDate;
        AmountPaid = amountPaid;
        PaymentMethodId = paymentMethodId;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }

    public VendorPayment Update(
        DateTime? paymentDate,
        decimal? amountPaid,
        Guid? paymentMethodId,
        string? referenceNumber,
        string? notes)
    {
        if (paymentDate.HasValue && PaymentDate != paymentDate.Value) PaymentDate = paymentDate.Value;
        if (amountPaid.HasValue && AmountPaid != amountPaid.Value) AmountPaid = amountPaid.Value;
        if (paymentMethodId.HasValue && PaymentMethodId != paymentMethodId.Value) PaymentMethodId = paymentMethodId.Value;
        if (referenceNumber is not null && ReferenceNumber?.Equals(referenceNumber) is not true) ReferenceNumber = referenceNumber;
        if (notes is not null && Notes?.Equals(notes) is not true) Notes = notes;
        return this;
    }

    public void AddPaymentApplication(VendorPaymentApplication application)
    {
        // Consider validation: e.g., application should not be null, application should belong to this payment
        _appliedInvoices.Add(application);
    }

    public void RemovePaymentApplication(Guid vendorPaymentApplicationId)
    {
        var application = _appliedInvoices.FirstOrDefault(a => a.Id == vendorPaymentApplicationId);
        if (application != null)
        {
            _appliedInvoices.Remove(application);
        }
    }

    public void ClearPaymentApplications()
    {
        _appliedInvoices.Clear();
    }
}
