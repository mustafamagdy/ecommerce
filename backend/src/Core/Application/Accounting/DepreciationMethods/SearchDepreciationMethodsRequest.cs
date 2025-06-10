using FSH.WebApi.Application.Common.Models;
using MediatR;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

// Even if they are few, providing a search request with pagination is consistent.
// Filters might be added later if needed (e.g., by keyword in name/description).
public class SearchDepreciationMethodsRequest : PaginationFilter, IRequest<PaginationResponse<DepreciationMethodDto>>
{
    public string? NameKeyword { get; set; }
}
