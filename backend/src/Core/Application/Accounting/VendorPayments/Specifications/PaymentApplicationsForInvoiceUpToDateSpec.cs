using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorPaymentApplication, VendorPayment
using System;
using System.Collections.Generic; // For IEnumerable
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorPayments.Specifications;

public class PaymentApplicationsForInvoicesUpToDateSpec : Specification<VendorPaymentApplication>
{
    // Fetches all payment applications for a list of vendorInvoiceIds where payment date is on or before asOfDate
    public PaymentApplicationsForInvoicesUpToDateSpec(IEnumerable<Guid> vendorInvoiceIds, DateTime asOfDate)
    {
        Query
            .Where(app => vendorInvoiceIds.Contains(app.VendorInvoiceId) &&
                          app.VendorPayment.PaymentDate <= asOfDate)
            .Include(app => app.VendorPayment); // Include VendorPayment to access PaymentDate
    }

    // Overload for a single invoice ID
    public PaymentApplicationsForInvoicesUpToDateSpec(Guid vendorInvoiceId, DateTime asOfDate)
    {
        Query
            .Where(app => app.VendorInvoiceId == vendorInvoiceId &&
                          app.VendorPayment.PaymentDate <= asOfDate)
            .Include(app => app.VendorPayment);
    }
}
