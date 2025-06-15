using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For FixedAsset, FixedAssetStatus, AssetCategory
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.FixedAssets.Specifications;

public class DisposedFixedAssetsForReportSpec : Specification<FixedAsset>
{
    public DisposedFixedAssetsForReportSpec(DateTime? startDate, DateTime? endDate, Guid? assetCategoryId)
    {
        Query
            .Where(fa => fa.Status == FixedAssetStatus.Disposed && fa.DisposalDate.HasValue) // Must be disposed and have a disposal date
            .Include(fa => fa.AssetCategory); // For AssetCategoryName
            // DepreciationMethod is not strictly needed for this report's DTO, but can be included if desired.
            // DepreciationEntries will be fetched separately and correlated.

        if (startDate.HasValue)
        {
            Query.Where(fa => fa.DisposalDate!.Value >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            Query.Where(fa => fa.DisposalDate!.Value <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }
        if (assetCategoryId.HasValue)
        {
            Query.Where(fa => fa.AssetCategoryId == assetCategoryId.Value);
        }

        Query.OrderBy(fa => fa.DisposalDate).ThenBy(fa => fa.AssetNumber); // Default sort
    }
}
