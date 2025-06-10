using FSH.WebApi.Application.Common.Models;
using MediatR;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class SearchSuppliersRequest : PaginationFilter, IRequest<PaginationResponse<SupplierDto>>
{
    public string? Keyword { get; set; }
    // Add other filterable properties if needed, for example:
    // public bool? IsActive { get; set; }
}
