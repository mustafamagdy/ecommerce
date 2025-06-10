using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class CreditMemoApplication : AuditableEntity
{
    public Guid CreditMemoId { get; private set; }
    // public virtual CreditMemo CreditMemo { get; private set; } = default!; // Navigation

    public Guid CustomerInvoiceId { get; private set; }
    // public virtual CustomerInvoice CustomerInvoice { get; private set; } = default!; // Navigation

    public decimal AmountApplied { get; private set; }

    // Private constructor for EF Core
    private CreditMemoApplication() { }

    public CreditMemoApplication(Guid creditMemoId, Guid customerInvoiceId, decimal amountApplied)
    {
        CreditMemoId = creditMemoId;
        CustomerInvoiceId = customerInvoiceId;

        if (amountApplied <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountApplied), "Amount applied must be positive.");
        AmountApplied = amountApplied;
    }

    // Usually, applications are not updated directly. They are removed and recreated if changes are needed.
    // public void UpdateAmountApplied(decimal newAmountApplied)
    // {
    //     if (newAmountApplied <= 0)
    //         throw new ArgumentOutOfRangeException(nameof(newAmountApplied), "New amount applied must be positive.");
    //     AmountApplied = newAmountApplied;
    // }
}
