using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class CustomerPaymentApplication : AuditableEntity
{
    public Guid CustomerPaymentId { get; private set; }
    // public virtual CustomerPayment CustomerPayment { get; private set; } = default!; // Navigation

    public Guid CustomerInvoiceId { get; private set; }
    // public virtual CustomerInvoice CustomerInvoice { get; private set; } = default!; // Navigation

    public decimal AmountApplied { get; private set; }

    // Private constructor for EF Core
    private CustomerPaymentApplication() { }

    public CustomerPaymentApplication(Guid customerPaymentId, Guid customerInvoiceId, decimal amountApplied)
    {
        CustomerPaymentId = customerPaymentId;
        CustomerInvoiceId = customerInvoiceId;

        if (amountApplied <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountApplied), "Amount applied must be positive.");
        AmountApplied = amountApplied;
    }

    public void UpdateAmountApplied(decimal newAmountApplied)
    {
        // This might be complex if it affects invoice status or payment's total applied amount.
        // Usually, applications are deleted and recreated rather than updated directly,
        // or this method would need to trigger re-validation on the parent CustomerPayment.
        if (newAmountApplied <= 0)
            throw new ArgumentOutOfRangeException(nameof(newAmountApplied), "New amount applied must be positive.");
        AmountApplied = newAmountApplied;
    }
}
