using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class AssetCategoryByIdWithDetailsSpec : Specification<AssetCategory, AssetCategoryDto>, ISingleResultSpecification
{
    public AssetCategoryByIdWithDetailsSpec(Guid assetCategoryId)
    {
        Query
            .Where(ac => ac.Id == assetCategoryId);
            // .Include(ac => ac.DefaultDepreciationMethod); // If direct navigation property exists and is needed for Name

        // Note: DefaultDepreciationMethodName will be populated in the handler by a separate query
        // if DefaultDepreciationMethod navigation property is not available or not desired to load full entity.
    }
}
