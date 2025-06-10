using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class FixedAssetByIdWithDetailsSpec : Specification<FixedAsset, FixedAssetDto>, ISingleResultSpecification
{
    public FixedAssetByIdWithDetailsSpec(Guid fixedAssetId)
    {
        Query
            .Where(fa => fa.Id == fixedAssetId);
            // .Include(fa => fa.AssetCategory)    // For AssetCategoryName
            // .Include(fa => fa.DepreciationMethod); // For DepreciationMethodName
        // Names will be populated in the handler via separate queries to their respective repositories.
    }
}
