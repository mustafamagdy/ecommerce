using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

// This spec is to fetch AssetDepreciationEntry entities directly.
public class AssetDepreciationHistorySpec : Specification<AssetDepreciationEntry, AssetDepreciationEntryDto>
{
    public AssetDepreciationHistorySpec(GetAssetDepreciationHistoryRequest request)
    {
        Query
            .Where(e => e.FixedAssetId == request.FixedAssetId)
            .OrderByDescending(e => e.DepreciationDate); // Show latest first

        if (request.DateFrom.HasValue)
        {
            Query.Where(e => e.DepreciationDate >= request.DateFrom.Value);
        }
        if (request.DateTo.HasValue)
        {
            Query.Where(e => e.DepreciationDate <= request.DateTo.Value.AddDays(1).AddTicks(-1));
        }

        // Handle pagination from PaginationFilter base class if this spec inherits from EntitiesByPaginationFilterSpec
        // If not, apply pagination manually if needed for direct use with ListAsync that supports skip/take.
        // However, GetAssetDepreciationHistoryRequest inherits PaginationFilter, so this spec should too.
        // For now, assuming the handler will use a method that supports PaginationFilter directly with this spec,
        // or this spec would need to be EntitiesByPaginationFilterSpec<AssetDepreciationEntry, AssetDepreciationEntryDto>.

        // Let's adjust to use EntitiesByPaginationFilterSpec for consistency
        // This requires GetAssetDepreciationHistoryRequest to be passed to base or its properties used.
        // The current constructor takes GetAssetDepreciationHistoryRequest, which is fine.
        // The base class EntitiesByPaginationFilterSpec will handle Skip & Take.
    }
}

// If EntitiesByPaginationFilterSpec is preferred for AssetDepreciationHistorySpec:
public class PaginatedAssetDepreciationHistorySpec : EntitiesByPaginationFilterSpec<AssetDepreciationEntry, AssetDepreciationEntryDto>
{
    public PaginatedAssetDepreciationHistorySpec(GetAssetDepreciationHistoryRequest request)
        : base(request) // This applies pagination via base class
    {
        Query
            .Where(e => e.FixedAssetId == request.FixedAssetId);

        if (!request.HasOrderBy()) // Apply default order if none specified in request (PaginationFilter usually has OrderBy)
        {
            Query.OrderByDescending(e => e.DepreciationDate);
        }


        if (request.DateFrom.HasValue)
        {
            Query.Where(e => e.DepreciationDate >= request.DateFrom.Value);
        }
        if (request.DateTo.HasValue)
        {
            // Assuming DateTo is inclusive of the day
            Query.Where(e => e.DepreciationDate < request.DateTo.Value.AddDays(1));
        }
    }
}
