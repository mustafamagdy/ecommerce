using FSH.WebApi.Application.Common.Models; // For PaginationFilter if SearchSuppliersRequest is not directly used
using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec
using FSH.WebApi.Domain.Accounting; // For Supplier
using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity (if base spec needs it)

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class SuppliersBySearchFilterSpec : EntitiesByPaginationFilterSpec<Supplier, SupplierDto>
{
    public SuppliersBySearchFilterSpec(SearchSuppliersRequest request)
        : base(request) // Pass the request to the base constructor
    {
        Query.OrderBy(s => s.Name, !request.HasOrderBy()); // Default order by Name

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Search(s => s.Name, "%" + request.Keyword + "%") // Search in Name
                 .Search(s => s.ContactInfo, "%" + request.Keyword + "%") // Search in ContactInfo
                 .Search(s => s.TaxId, "%" + request.Keyword + "%"); // Search in TaxId
        }

        // Example for filtering by a boolean property, if one existed (e.g., IsActive)
        // if (request.IsActive.HasValue)
        // {
        //     Query.Where(s => s.IsActive == request.IsActive.Value);
        // }
    }
}
