using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.FixedAssets.Specifications; // For the new specs
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class FixedAssetListingHandler : IRequestHandler<FixedAssetListingRequest, FixedAssetListingDto>
{
    private readonly IReadRepository<FixedAsset> _assetRepo;
    private readonly IReadRepository<AssetCategory> _categoryRepo;
    // DepreciationMethod details are included in FixedAsset via spec, so direct repo might not be needed here for names.
    // private readonly IReadRepository<DepreciationMethod> _methodRepo;
    private readonly IReadRepository<AssetDepreciationEntry> _depreciationEntryRepo;
    // private readonly IStringLocalizer<FixedAssetListingHandler> _localizer;

    public FixedAssetListingHandler(
        IReadRepository<FixedAsset> assetRepo,
        IReadRepository<AssetCategory> categoryRepo,
        // IReadRepository<DepreciationMethod> methodRepo,
        IReadRepository<AssetDepreciationEntry> depreciationEntryRepo
        /* IStringLocalizer<FixedAssetListingHandler> localizer */)
    {
        _assetRepo = assetRepo;
        _categoryRepo = categoryRepo;
        // _methodRepo = methodRepo;
        _depreciationEntryRepo = depreciationEntryRepo;
        // _localizer = localizer;
    }

    public async Task<FixedAssetListingDto> Handle(FixedAssetListingRequest request, CancellationToken cancellationToken)
    {
        DateTime asOfDateForCalc = request.AsOfDate ?? DateTime.UtcNow;

        var reportDto = new FixedAssetListingDto
        {
            AssetCategoryId = request.AssetCategoryId,
            StatusFilter = request.Status,
            LocationFilter = request.LocationKeyword,
            AsOfDate = asOfDateForCalc, // Reflect the date used for calculations
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Assets = new List<FixedAssetListingLineDto>()
        };

        if (request.AssetCategoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(request.AssetCategoryId.Value, cancellationToken);
            if (category == null)
                throw new NotFoundException($"Asset Category with ID {request.AssetCategoryId.Value} not found.");
            reportDto.AssetCategoryName = category.Name;
        }

        // 1. Fetch Fixed Assets
        // Pass AsOfDate to spec for filtering assets that existed at that time
        var assetsSpec = new FixedAssetsForListingSpec(request.AssetCategoryId, request.Status, request.LocationKeyword, asOfDateForCalc);
        var fixedAssets = await _assetRepo.ListAsync(assetsSpec, cancellationToken);

        if (!fixedAssets.Any())
        {
            return reportDto; // Return empty report
        }

        // 2. Fetch Relevant Depreciation Entries (Optimization)
        var assetIds = fixedAssets.Select(fa => fa.Id).ToList();
        var depreciationEntriesSpec = new DepreciationEntriesForAssetsUpToDateSpec(assetIds, asOfDateForCalc);
        var allDepreciationEntries = await _depreciationEntryRepo.ListAsync(depreciationEntriesSpec, cancellationToken);
        var entriesByAssetId = allDepreciationEntries
            .GroupBy(e => e.FixedAssetId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 3. Populate Report Lines
        reportDto.GrandTotalPurchaseCost = 0m;
        reportDto.GrandTotalAccumulatedDepreciation = 0m;
        reportDto.GrandTotalBookValue = 0m;

        foreach (var asset in fixedAssets)
        {
            decimal accumulatedDepreciation = 0m;
            if (entriesByAssetId.TryGetValue(asset.Id, out var entries))
            {
                accumulatedDepreciation = entries.Sum(e => e.Amount);
            }

            // Ensure accumulated depreciation does not exceed depreciable base
            decimal depreciableBase = asset.PurchaseCost - asset.SalvageValue;
            if (accumulatedDepreciation > depreciableBase)
            {
                accumulatedDepreciation = depreciableBase;
            }
             if (accumulatedDepreciation < 0) accumulatedDepreciation = 0; // Should not happen with valid entries


            decimal bookValue = asset.PurchaseCost - accumulatedDepreciation;

            var line = new FixedAssetListingLineDto
            {
                FixedAssetId = asset.Id,
                AssetNumber = asset.AssetNumber,
                AssetName = asset.AssetName,
                Description = asset.Description,
                AssetCategoryId = asset.AssetCategoryId,
                AssetCategoryName = asset.AssetCategory?.Name ?? "N/A", // AssetCategory included by spec
                PurchaseDate = asset.PurchaseDate,
                PurchaseCost = asset.PurchaseCost,
                SalvageValue = asset.SalvageValue,
                UsefulLifeYears = asset.UsefulLifeYears,
                DepreciationMethodId = asset.DepreciationMethodId,
                DepreciationMethodName = asset.DepreciationMethod?.Name ?? "N/A", // DepreciationMethod included
                AccumulatedDepreciation = accumulatedDepreciation,
                BookValue = bookValue,
                Status = asset.Status.ToString(),
                Location = asset.Location,
                DisposalDate = asset.DisposalDate
            };
            reportDto.Assets.Add(line);

            reportDto.GrandTotalPurchaseCost += asset.PurchaseCost;
            reportDto.GrandTotalAccumulatedDepreciation += accumulatedDepreciation;
            reportDto.GrandTotalBookValue += bookValue;
        }

        // Sorting is handled by the spec (AssetNumber ASC)
        // reportDto.Assets = reportDto.Assets.OrderBy(a => a.AssetNumber).ToList();

        return reportDto;
    }
}
