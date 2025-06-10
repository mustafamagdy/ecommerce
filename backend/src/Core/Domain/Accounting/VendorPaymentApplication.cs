using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class VendorPaymentApplication : AuditableEntity
{
    public Guid VendorPaymentId { get; private set; }
    public virtual VendorPayment VendorPayment { get; private set; } = default!; // Navigation property

    public Guid VendorInvoiceId { get; private set; }
    public virtual VendorInvoice VendorInvoice { get; private set; } = default!; // Navigation property

    public decimal AmountApplied { get; private set; }

    public VendorPaymentApplication(Guid vendorPaymentId, Guid vendorInvoiceId, decimal amountApplied)
    {
        VendorPaymentId = vendorPaymentId;
        VendorInvoiceId = vendorInvoiceId;
        AmountApplied = amountApplied; // Consider validation: AmountApplied should not exceed invoice balance or payment remaining amount
    }

    public VendorPaymentApplication Update(decimal? amountApplied)
    {
        if (amountApplied.HasValue && AmountApplied != amountApplied.Value)
        {
            // Add validation if necessary, e.g. cannot apply negative amount
            AmountApplied = amountApplied.Value;
        }
        return this;
    }
}
