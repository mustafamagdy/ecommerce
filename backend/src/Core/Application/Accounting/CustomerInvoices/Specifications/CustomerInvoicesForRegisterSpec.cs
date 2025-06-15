using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CustomerInvoice, CustomerInvoiceStatus
using FSH.WebApi.Domain.Operation.Customers; // For Customer
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices.Specifications;

public class CustomerInvoicesForRegisterSpec : Specification<CustomerInvoice>
{
    public CustomerInvoicesForRegisterSpec(DateTime? startDate, DateTime? endDate, Guid? customerId)
    {
        Query
            .Include(ci => ci.Customer); // Include Customer for CustomerName and ContactInfo

        if (startDate.HasValue)
        {
            Query.Where(ci => ci.InvoiceDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(ci => ci.InvoiceDate <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }

        if (customerId.HasValue)
        {
            Query.Where(ci => ci.CustomerId == customerId.Value);
        }

        // Exclude Void and Draft invoices from the register by default
        Query.Where(ci => ci.Status != CustomerInvoiceStatus.Void && ci.Status != CustomerInvoiceStatus.Draft);

        // Default sort order
        Query.OrderBy(ci => ci.Customer.Name) // Requires Customer to be non-null. Add null check or ensure CustomerId always valid.
             .ThenBy(ci => ci.InvoiceDate);
    }
}
