using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class DepreciationMethodsBySearchFilterSpec : EntitiesByPaginationFilterSpec<DepreciationMethod, DepreciationMethodDto>
{
    public DepreciationMethodsBySearchFilterSpec(SearchDepreciationMethodsRequest request)
        : base(request)
    {
        Query.OrderBy(dm => dm.Name, !request.HasOrderBy());

        if (!string.IsNullOrEmpty(request.NameKeyword))
        {
            Query.Search(dm => dm.Name, "%" + request.NameKeyword + "%")
                 .Search(dm => dm.Description, "%" + request.NameKeyword + "%");
        }
    }
}
