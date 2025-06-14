using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CustomerInvoice, CustomerInvoiceStatus
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices.Specifications;

public class CustomerInvoicesForAgingSpec : Specification<CustomerInvoice>
{
    public CustomerInvoicesForAgingSpec(DateTime asOfDate, Guid? customerId)
    {
        Query
            .Where(ci => ci.InvoiceDate <= asOfDate &&
                         ci.Status != CustomerInvoiceStatus.Void && // Exclude Void invoices
                         ci.Status != CustomerInvoiceStatus.Draft)   // Exclude Draft invoices
            .Include(ci => ci.Customer); // Include Customer for CustomerName

        if (customerId.HasValue)
        {
            Query.Where(ci => ci.CustomerId == customerId.Value);
        }

        // Similar to AP Aging, the "not fully paid" check is complex for a point-in-time DB query.
        // The handler will calculate paid amounts as of AsOfDate and filter accordingly.
    }
}
