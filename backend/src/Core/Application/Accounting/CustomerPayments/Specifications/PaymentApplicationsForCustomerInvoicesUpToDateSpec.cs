using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CustomerPaymentApplication, CustomerPayment
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerPayments.Specifications;

public class PaymentApplicationsForCustomerInvoicesUpToDateSpec : Specification<CustomerPaymentApplication>
{
    // Fetches all payment applications for a list of customerInvoiceIds where payment date is on or before asOfDate
    public PaymentApplicationsForCustomerInvoicesUpToDateSpec(IEnumerable<Guid> customerInvoiceIds, DateTime asOfDate)
    {
        Query
            .Where(app => customerInvoiceIds.Contains(app.CustomerInvoiceId) &&
                          app.CustomerPayment.PaymentDate <= asOfDate)
            .Include(app => app.CustomerPayment); // Include CustomerPayment to access PaymentDate
    }

    // Overload for a single invoice ID, if needed, though less likely for batch processing
    public PaymentApplicationsForCustomerInvoicesUpToDateSpec(Guid customerInvoiceId, DateTime asOfDate)
    {
        Query
            .Where(app => app.CustomerInvoiceId == customerInvoiceId &&
                          app.CustomerPayment.PaymentDate <= asOfDate)
            .Include(app => app.CustomerPayment);
    }
}
