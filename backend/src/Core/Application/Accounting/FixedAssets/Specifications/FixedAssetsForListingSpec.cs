using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For FixedAsset, FixedAssetStatus, AssetCategory, DepreciationMethod
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.FixedAssets.Specifications;

public class FixedAssetsForListingSpec : Specification<FixedAsset>
{
    public FixedAssetsForListingSpec(Guid? assetCategoryId, string? statusString, string? locationKeyword, DateTime? asOfDate)
    {
        Query
            .Include(fa => fa.AssetCategory)
            .Include(fa => fa.DepreciationMethod);

        if (assetCategoryId.HasValue)
        {
            Query.Where(fa => fa.AssetCategoryId == assetCategoryId.Value);
        }

        if (!string.IsNullOrEmpty(statusString) && Enum.TryParse<FixedAssetStatus>(statusString, true, out var statusEnum))
        {
            // Allow "All" or similar if passed, otherwise filter by specific status
            // For this spec, if a valid status string is passed, we filter by it.
            // If statusString is "All" or invalid, no status filter is applied by this part.
            // The handler might decide not to pass the filter if "All" is selected by user.
            Query.Where(fa => fa.Status == statusEnum);
        }
        // Else, if statusString is null/empty or invalid, all statuses are included (except as filtered below)

        if (!string.IsNullOrEmpty(locationKeyword))
        {
            Query.Search(fa => fa.Location, "%" + locationKeyword + "%");
        }

        // For a listing "as of date", we usually want assets that existed (were purchased) on or before that date
        // And potentially weren't disposed *before* that date if we are strict.
        // If AsOfDate is for calculating depreciation, all assets that could have depreciation are relevant.
        // If it's for a snapshot of assets "owned" at AsOfDate:
        if (asOfDate.HasValue)
        {
            Query.Where(fa => fa.PurchaseDate <= asOfDate.Value &&
                               (fa.DisposalDate == null || fa.DisposalDate.Value > asOfDate.Value));
        }


        Query.OrderBy(fa => fa.AssetNumber); // Default sort
    }
}
