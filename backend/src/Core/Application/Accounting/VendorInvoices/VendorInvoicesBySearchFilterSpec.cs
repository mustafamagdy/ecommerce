using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorInvoice, VendorInvoiceStatus
using System; // For Enum.TryParse
using LinqKit; // For PredicateBuilder if complex OR logic is needed, may require package.

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class VendorInvoicesBySearchFilterSpec : EntitiesByPaginationFilterSpec<VendorInvoice, VendorInvoiceDto>
{
    public VendorInvoicesBySearchFilterSpec(SearchVendorInvoicesRequest request)
        : base(request)
    {
        Query.OrderByDescending(vi => vi.InvoiceDate, !request.HasOrderBy()); // Default order

        if (request.SupplierId.HasValue)
        {
            Query.Where(vi => vi.SupplierId == request.SupplierId.Value);
        }

        if (!string.IsNullOrEmpty(request.InvoiceStatus))
        {
            if (Enum.TryParse<VendorInvoiceStatus>(request.InvoiceStatus, true, out var statusEnum))
            {
                Query.Where(vi => vi.Status == statusEnum);
            }
            // else: handle invalid status string? Or ignore? Currently ignores.
        }

        if (request.DateFrom.HasValue)
        {
            Query.Where(vi => vi.InvoiceDate >= request.DateFrom.Value);
        }
        if (request.DateTo.HasValue)
        {
            Query.Where(vi => vi.InvoiceDate <= request.DateTo.Value.AddDays(1).AddTicks(-1)); // Include whole day
        }

        if (request.DueDateFrom.HasValue)
        {
            Query.Where(vi => vi.DueDate >= request.DueDateFrom.Value);
        }
        if (request.DueDateTo.HasValue)
        {
            Query.Where(vi => vi.DueDate <= request.DueDateTo.Value.AddDays(1).AddTicks(-1)); // Include whole day
        }

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            // Using PredicateBuilder for OR conditions if searching across multiple fields, including Supplier.Name
            // This assumes Supplier navigation property is loaded or query can handle it (e.g. EF Core includes).
            // var predicate = PredicateBuilder.New<VendorInvoice>(true); // true for AND, false for OR as initial
            // predicate = predicate.Or(vi => vi.InvoiceNumber.Contains(request.Keyword));
            // predicate = predicate.Or(vi => vi.Supplier.Name.Contains(request.Keyword)); // Requires join
            // Query.Where(predicate);

            // Simpler approach if only searching InvoiceNumber:
             Query.Search(vi => vi.InvoiceNumber, "%" + request.Keyword + "%");
            // If searching Supplier Name, it's more complex and might require a custom spec that handles joins
            // or the repository needs to support Include(vi => vi.Supplier) for the search to work.
            // For now, focusing on direct properties of VendorInvoice.
            // Query.Where(vi => vi.InvoiceNumber.Contains(request.Keyword) /* || vi.Supplier.Name.Contains(request.Keyword) */ );
        }
    }
}
