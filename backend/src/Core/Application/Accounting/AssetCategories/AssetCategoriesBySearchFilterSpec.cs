using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;
using System; // Required for Guid?

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class AssetCategoriesBySearchFilterSpec : EntitiesByPaginationFilterSpec<AssetCategory, AssetCategoryDto>
{
    public AssetCategoriesBySearchFilterSpec(SearchAssetCategoriesRequest request)
        : base(request)
    {
        Query.OrderBy(ac => ac.Name, !request.HasOrderBy());

        if (!string.IsNullOrEmpty(request.NameKeyword))
        {
            Query.Search(ac => ac.Name, "%" + request.NameKeyword + "%")
                 .Search(ac => ac.Description, "%" + request.NameKeyword + "%");
        }

        if (request.IsActive.HasValue)
        {
            Query.Where(ac => ac.IsActive == request.IsActive.Value);
        }

        if (request.DefaultDepreciationMethodId.HasValue)
        {
            Query.Where(ac => ac.DefaultDepreciationMethodId == request.DefaultDepreciationMethodId.Value);
        }

        // If DefaultDepreciationMethod navigation property is available and you want to include it for all search results:
        // Query.Include(ac => ac.DefaultDepreciationMethod);
    }
}
