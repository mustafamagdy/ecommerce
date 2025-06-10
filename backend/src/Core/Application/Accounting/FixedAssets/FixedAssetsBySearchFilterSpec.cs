using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For FixedAsset, FixedAssetStatus
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class FixedAssetsBySearchFilterSpec : EntitiesByPaginationFilterSpec<FixedAsset, FixedAssetDto>
{
    public FixedAssetsBySearchFilterSpec(SearchFixedAssetsRequest request)
        : base(request)
    {
        Query.OrderBy(fa => fa.AssetName, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.AssetNumberKeyword))
        {
            Query.Search(fa => fa.AssetNumber, "%" + request.AssetNumberKeyword + "%");
        }

        if (!string.IsNullOrEmpty(request.AssetNameKeyword))
        {
            Query.Search(fa => fa.AssetName, "%" + request.AssetNameKeyword + "%")
                 .Search(fa => fa.Description, "%" + request.AssetNameKeyword + "%");
        }

        if (request.AssetCategoryId.HasValue)
        {
            Query.Where(fa => fa.AssetCategoryId == request.AssetCategoryId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<FixedAssetStatus>(request.Status, true, out var statusEnum))
            {
                Query.Where(fa => fa.Status == statusEnum);
            }
        }

        if (request.PurchaseDateFrom.HasValue)
        {
            Query.Where(fa => fa.PurchaseDate >= request.PurchaseDateFrom.Value);
        }
        if (request.PurchaseDateTo.HasValue)
        {
            Query.Where(fa => fa.PurchaseDate <= request.PurchaseDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (request.DepreciationMethodId.HasValue)
        {
            Query.Where(fa => fa.DepreciationMethodId == request.DepreciationMethodId.Value);
        }

        if(!string.IsNullOrEmpty(request.LocationKeyword))
        {
            Query.Search(fa => fa.Location, "%" + request.LocationKeyword + "%");
        }

        // For list view, we might not need to include AssetCategory and DepreciationMethod
        // if their names are populated by separate queries in the handler (more efficient for large lists).
        // If direct inclusion is preferred and performance allows:
        // Query.Include(fa => fa.AssetCategory).Include(fa => fa.DepreciationMethod);
    }
}
