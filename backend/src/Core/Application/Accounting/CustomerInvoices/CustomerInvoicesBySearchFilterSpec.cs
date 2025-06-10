using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For CustomerInvoice, CustomerInvoiceStatus
using System;
using LinqKit; // Optional, for complex OR logic if needed

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class CustomerInvoicesBySearchFilterSpec : EntitiesByPaginationFilterSpec<CustomerInvoice, CustomerInvoiceDto>
{
    public CustomerInvoicesBySearchFilterSpec(SearchCustomerInvoicesRequest request)
        : base(request)
    {
        Query.OrderByDescending(ci => ci.InvoiceDate, !request.HasOrderBy()); // Default order

        if (request.CustomerId.HasValue)
        {
            Query.Where(ci => ci.CustomerId == request.CustomerId.Value);
        }

        if (request.OrderId.HasValue)
        {
            Query.Where(ci => ci.OrderId == request.OrderId.Value);
        }

        if (!string.IsNullOrEmpty(request.InvoiceNumberKeyword))
        {
            Query.Search(ci => ci.InvoiceNumber, "%" + request.InvoiceNumberKeyword + "%");
        }

        if (request.InvoiceDateFrom.HasValue)
        {
            Query.Where(ci => ci.InvoiceDate >= request.InvoiceDateFrom.Value);
        }
        if (request.InvoiceDateTo.HasValue)
        {
            Query.Where(ci => ci.InvoiceDate <= request.InvoiceDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (request.DueDateFrom.HasValue)
        {
            Query.Where(ci => ci.DueDate >= request.DueDateFrom.Value);
        }
        if (request.DueDateTo.HasValue)
        {
            Query.Where(ci => ci.DueDate <= request.DueDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<CustomerInvoiceStatus>(request.Status, true, out var statusEnum))
            {
                Query.Where(ci => ci.Status == statusEnum);
            }
        }

        if (request.MinimumAmount.HasValue)
        {
            Query.Where(ci => ci.TotalAmount >= request.MinimumAmount.Value);
        }
        if (request.MaximumAmount.HasValue)
        {
            Query.Where(ci => ci.TotalAmount <= request.MaximumAmount.Value);
        }

        // Ensure InvoiceItems are included if they're part of the DTO and needed for list view (e.g. item count)
        // For full item details in a list, it's often better to fetch them on demand (when expanding a row)
        // or ensure the DTO for list view is lightweight.
        // If CustomerInvoiceDto in list view needs InvoiceItems, then include them:
        Query.Include(ci => ci.InvoiceItems);
    }
}
