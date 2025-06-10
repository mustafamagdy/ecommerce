using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Operation.Customers; // Assuming path for Customer
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Domain.Accounting;

public class CustomerPayment : AuditableEntity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }
    // public virtual Customer Customer { get; private set; } = default!; // Navigation

    public DateTime PaymentDate { get; private set; }
    public decimal AmountReceived { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    // public virtual PaymentMethod PaymentMethod { get; private set; } = default!; // Navigation

    public string? ReferenceNumber { get; private set; } // E.g., check number, transaction ID
    public string? Notes { get; private set; }

    private readonly List<CustomerPaymentApplication> _appliedInvoices = new();
    public IReadOnlyCollection<CustomerPaymentApplication> AppliedInvoices => _appliedInvoices.AsReadOnly();

    // Private constructor for EF Core
    private CustomerPayment() { }

    public CustomerPayment(
        Guid customerId,
        DateTime paymentDate,
        decimal amountReceived,
        Guid paymentMethodId,
        string? referenceNumber = null,
        string? notes = null)
    {
        CustomerId = customerId;
        PaymentDate = paymentDate;
        AmountReceived = amountReceived;
        PaymentMethodId = paymentMethodId;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }

    public void AddPaymentApplication(Guid customerInvoiceId, decimal amountToApply)
    {
        if (amountToApply <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountToApply), "Amount to apply must be positive.");

        // Ensure not over-applying the payment itself
        decimal totalCurrentlyApplied = _appliedInvoices.Sum(app => app.AmountApplied);
        if (totalCurrentlyApplied + amountToApply > AmountReceived)
            throw new InvalidOperationException("Cannot apply more than the total amount received for the payment.");

        var application = new CustomerPaymentApplication(this.Id, customerInvoiceId, amountToApply);
        _appliedInvoices.Add(application);
    }

    public void RemovePaymentApplication(Guid customerPaymentApplicationId)
    {
        var application = _appliedInvoices.FirstOrDefault(a => a.Id == customerPaymentApplicationId);
        if (application != null)
        {
            _appliedInvoices.Remove(application);
        }
    }

    public decimal GetUnappliedAmount()
    {
        return AmountReceived - _appliedInvoices.Sum(app => app.AmountApplied);
    }

    public void Update(DateTime? paymentDate, decimal? amountReceived, Guid? paymentMethodId, string? referenceNumber, string? notes)
    {
        // Business rule: AmountReceived probably shouldn't be changed if applications exist and sum up to old amount.
        // Or, if it changes, applications might need re-validation. For simplicity, allow update but be wary.
        if (amountReceived.HasValue && _appliedInvoices.Any() && amountReceived.Value < _appliedInvoices.Sum(a => a.AmountApplied))
        {
            throw new InvalidOperationException("New amount received cannot be less than the total amount already applied to invoices.");
        }


        if (paymentDate.HasValue) PaymentDate = paymentDate.Value;
        if (amountReceived.HasValue) AmountReceived = amountReceived.Value;
        if (paymentMethodId.HasValue) PaymentMethodId = paymentMethodId.Value;
        if (referenceNumber is not null) ReferenceNumber = referenceNumber; // Allow setting to empty
        if (notes is not null) Notes = notes; // Allow setting to empty
    }
}
