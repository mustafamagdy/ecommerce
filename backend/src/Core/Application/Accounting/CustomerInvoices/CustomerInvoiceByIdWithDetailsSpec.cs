using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

// Specification to fetch a CustomerInvoice with its items, and potentially related Customer/Order names
public class CustomerInvoiceByIdWithDetailsSpec : Specification<CustomerInvoice, CustomerInvoiceDto>, ISingleResultSpecification
{
    public CustomerInvoiceByIdWithDetailsSpec(Guid customerInvoiceId)
    {
        Query
            .Where(ci => ci.Id == customerInvoiceId)
            .Include(ci => ci.InvoiceItems);
            // .Include(ci => ci.Customer) // If Customer navigation property exists and is needed
            // .Include(ci => ci.Order);   // If Order navigation property exists and is needed

        // Note: Populating CustomerName and OrderNumber might be done in the handler
        // by separate queries to respective repositories if Customer/Order are in different bounded contexts
        // or if their full entities aren't needed.
    }
}
