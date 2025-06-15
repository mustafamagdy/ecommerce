using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For FixedAsset, AssetCategory, DepreciationMethod
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.FixedAssets.Specifications;

public class FixedAssetsForDepreciationScheduleSpec : Specification<FixedAsset>
{
    public FixedAssetsForDepreciationScheduleSpec(DateTime periodStartDate, DateTime periodEndDate, Guid? assetCategoryId, Guid? fixedAssetId)
    {
        Query
            .Include(fa => fa.AssetCategory)
            .Include(fa => fa.DepreciationMethod)
            // Include existing depreciation entries up to the period end date to help calculate start/period amounts
            .Include(fa => fa.DepreciationEntries.Where(de => de.DepreciationDate <= periodEndDate));


        if (fixedAssetId.HasValue)
        {
            Query.Where(fa => fa.Id == fixedAssetId.Value);
        }
        else if (assetCategoryId.HasValue)
        {
            Query.Where(fa => fa.AssetCategoryId == assetCategoryId.Value);
        }

        // Assets should be active at some point during the period, or purchased before period end,
        // and not disposed of before period start.
        Query.Where(fa =>
            fa.PurchaseDate <= periodEndDate &&
            (fa.DisposalDate == null || fa.DisposalDate.Value >= periodStartDate) &&
            fa.Status != FixedAssetStatus.UnderConstruction // Typically don't depreciate assets UC
            );
        // Further filtering by status (e.g. only Active) could be added if needed, but the above date logic is key.

        Query.OrderBy(fa => fa.AssetNumber); // Default sort
    }
}
