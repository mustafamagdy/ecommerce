using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.Operation.Customers; // Assuming path for Customer
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Domain.Accounting;

public enum CreditMemoStatus
{
    Draft,
    Approved, // Ready to be applied
    PartiallyApplied,
    Applied,  // Fully applied
    Void
}

public class CreditMemo : AuditableEntity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }
    // public virtual Customer Customer { get; private set; } = default!; // Navigation

    public string CreditMemoNumber { get; private set; } = default!; // Should be unique
    public DateTime Date { get; private set; }
    public string Reason { get; private set; } = default!;
    public decimal TotalAmount { get; private set; } // Total value of the credit memo
    public string Currency { get; private set; } = default!; // E.g., "USD", "EUR"
    public CreditMemoStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid? OriginalCustomerInvoiceId { get; private set; } // Optional: Link to an original invoice
    // public virtual CustomerInvoice? OriginalCustomerInvoice { get; private set; } // Navigation

    private readonly List<CreditMemoApplication> _applications = new();
    public IReadOnlyCollection<CreditMemoApplication> Applications => _applications.AsReadOnly();

    // Private constructor for EF Core
    private CreditMemo() { }

    public CreditMemo(
        Guid customerId,
        string creditMemoNumber,
        DateTime date,
        string reason,
        decimal totalAmount,
        string currency,
        string? notes = null,
        Guid? originalCustomerInvoiceId = null,
        CreditMemoStatus status = CreditMemoStatus.Draft)
    {
        CustomerId = customerId;
        CreditMemoNumber = creditMemoNumber;
        Date = date;
        Reason = reason;
        TotalAmount = totalAmount > 0 ? totalAmount : throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be positive.");
        Currency = currency;
        Notes = notes;
        OriginalCustomerInvoiceId = originalCustomerInvoiceId;
        Status = status;
    }

    public decimal GetAppliedAmount()
    {
        return _applications.Sum(app => app.AmountApplied);
    }

    public decimal GetAvailableBalance()
    {
        return TotalAmount - GetAppliedAmount();
    }

    public void AddApplication(Guid customerInvoiceId, decimal amountToApply)
    {
        if (amountToApply <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountToApply), "Amount to apply must be positive.");
        if (amountToApply > GetAvailableBalance())
            throw new InvalidOperationException("Amount to apply exceeds available credit balance.");

        var application = new CreditMemoApplication(this.Id, customerInvoiceId, amountToApply);
        _applications.Add(application);
        UpdateStatusAfterApplication();
    }

    public void RemoveApplication(Guid creditMemoApplicationId)
    {
        var application = _applications.FirstOrDefault(a => a.Id == creditMemoApplicationId);
        if (application != null)
        {
            _applications.Remove(application);
            UpdateStatusAfterApplication();
        }
    }

    private void UpdateStatusAfterApplication()
    {
        if (Status == CreditMemoStatus.Void || Status == CreditMemoStatus.Draft) return; // Don't change status if Void or still Draft

        var balance = GetAvailableBalance();
        if (balance <= 0)
        {
            Status = CreditMemoStatus.Applied;
        }
        else if (balance < TotalAmount)
        {
            Status = CreditMemoStatus.PartiallyApplied;
        }
        else // balance == TotalAmount (and not Draft)
        {
             Status = CreditMemoStatus.Approved; // Or whatever the initial "ready" state is if not Draft
        }
    }

    public void Approve() // Example of a status transition method
    {
        if (Status == CreditMemoStatus.Draft)
        {
            Status = CreditMemoStatus.Approved;
        }
        else
        {
            throw new InvalidOperationException($"Cannot approve a credit memo with status {Status}.");
        }
    }

    public void Void()
    {
        if (Status == CreditMemoStatus.Applied || Status == CreditMemoStatus.PartiallyApplied)
        {
            // Business Rule: Cannot void if already applied. May need un-application logic first.
            throw new InvalidOperationException($"Cannot void a credit memo that has been partially or fully applied. Unapply first or handle applied amounts.");
        }
        Status = CreditMemoStatus.Void;
    }


    public void Update(DateTime? date, string? reason, decimal? totalAmount, string? currency, string? notes, Guid? originalCustomerInvoiceId)
    {
        // Business rule: Cannot change TotalAmount if already applied.
        if (totalAmount.HasValue && GetAppliedAmount() > 0 && totalAmount.Value != TotalAmount)
        {
            throw new InvalidOperationException("Cannot change total amount after credit memo has been applied.");
        }
        if (totalAmount.HasValue && totalAmount.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be positive.");
        }


        if (date.HasValue) Date = date.Value;
        if (reason is not null) Reason = reason;
        if (totalAmount.HasValue) TotalAmount = totalAmount.Value;
        if (currency is not null) Currency = currency;
        if (notes is not null) Notes = notes;
        if (originalCustomerInvoiceId.HasValue) OriginalCustomerInvoiceId = originalCustomerInvoiceId.Value;
        // Status is typically not updated directly via this method, but through specific actions like Approve, Apply, Void.
    }
}
