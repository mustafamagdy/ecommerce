using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For AssetDepreciationEntry
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.FixedAssets.Specifications;

public class DepreciationEntriesForAssetsUpToDateSpec : Specification<AssetDepreciationEntry>
{
    public DepreciationEntriesForAssetsUpToDateSpec(IEnumerable<Guid> fixedAssetIds, DateTime asOfDate)
    {
        Query
            .Where(entry => fixedAssetIds.Contains(entry.FixedAssetId) &&
                            entry.DepreciationDate <= asOfDate);
        // No Includes needed as AssetDepreciationEntry is flat for this purpose.
        // OrderBy might be useful if processing entries chronologically for some reason, but Sum doesn't require it.
    }

    // Overload for a single FixedAssetId if ever needed, though batch is preferred for report
    public DepreciationEntriesForAssetsUpToDateSpec(Guid fixedAssetId, DateTime asOfDate)
    {
        Query
            .Where(entry => entry.FixedAssetId == fixedAssetId &&
                            entry.DepreciationDate <= asOfDate);
    }
}
