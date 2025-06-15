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

public class AssetDisposalReportHandler : IRequestHandler<AssetDisposalReportRequest, AssetDisposalReportDto>
{
    private readonly IReadRepository<FixedAsset> _assetRepo;
    private readonly IReadRepository<AssetCategory> _categoryRepo;
    private readonly IReadRepository<AssetDepreciationEntry> _depreciationEntryRepo;
    // private readonly IStringLocalizer<AssetDisposalReportHandler> _localizer;

    public AssetDisposalReportHandler(
        IReadRepository<FixedAsset> assetRepo,
        IReadRepository<AssetCategory> categoryRepo,
        IReadRepository<AssetDepreciationEntry> depreciationEntryRepo
        /* IStringLocalizer<AssetDisposalReportHandler> localizer */)
    {
        _assetRepo = assetRepo;
        _categoryRepo = categoryRepo;
        _depreciationEntryRepo = depreciationEntryRepo;
        // _localizer = localizer;
    }

    public async Task<AssetDisposalReportDto> Handle(AssetDisposalReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new AssetDisposalReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AssetCategoryId = request.AssetCategoryId,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Disposals = new List<AssetDisposalReportLineDto>()
        };

        if (request.AssetCategoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(request.AssetCategoryId.Value, cancellationToken);
            if (category == null)
                throw new NotFoundException($"Asset Category with ID {request.AssetCategoryId.Value} not found.");
            reportDto.AssetCategoryName = category.Name;
        }

        // 1. Fetch Disposed Fixed Assets
        var assetsSpec = new DisposedFixedAssetsForReportSpec(request.StartDate, request.EndDate, request.AssetCategoryId);
        var disposedAssets = await _assetRepo.ListAsync(assetsSpec, cancellationToken);

        if (!disposedAssets.Any())
        {
            return reportDto; // Return empty report
        }

        // 2. Fetch Relevant Depreciation Entries for all found assets up to their respective disposal dates
        var assetIds = disposedAssets.Select(fa => fa.Id).ToList();
        // We need all entries for these assets to calculate accumulated depreciation up to each asset's disposal date.
        // Fetching all entries up to the latest possible disposal date in the request, then filtering in memory.
        DateTime maxDisposalDate = request.EndDate ?? disposedAssets.Max(fa => fa.DisposalDate) ?? DateTime.UtcNow;
        var depreciationEntriesSpec = new DepreciationEntriesForAssetsUpToDateSpec(assetIds, maxDisposalDate);
        var allDepreciationEntries = await _depreciationEntryRepo.ListAsync(depreciationEntriesSpec, cancellationToken);

        var entriesByAssetId = allDepreciationEntries
            .GroupBy(e => e.FixedAssetId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 3. Populate Report Lines
        reportDto.GrandTotalOriginalCost = 0m;
        reportDto.GrandTotalAccumulatedDepreciationAtDisposal = 0m;
        reportDto.GrandTotalDisposalAmount = 0m;
        reportDto.GrandTotalGainLossOnDisposal = 0m;

        foreach (var asset in disposedAssets)
        {
            if (!asset.DisposalDate.HasValue) continue; // Should be filtered by spec, but as a safeguard

            decimal accumulatedDepreciationAtDisposal = 0m;
            if (entriesByAssetId.TryGetValue(asset.Id, out var entriesForThisAsset))
            {
                accumulatedDepreciationAtDisposal = entriesForThisAsset
                    .Where(e => e.DepreciationDate <= asset.DisposalDate.Value) // Ensure entries are up to disposal date
                    .Sum(e => e.Amount);
            }

            // Ensure accumulated depreciation does not exceed depreciable base
            decimal depreciableBase = asset.PurchaseCost - asset.SalvageValue;
            if (accumulatedDepreciationAtDisposal > depreciableBase && depreciableBase >=0)
            {
                accumulatedDepreciationAtDisposal = depreciableBase;
            }
            if (accumulatedDepreciationAtDisposal < 0) accumulatedDepreciationAtDisposal = 0;


            decimal bookValueAtDisposal = asset.PurchaseCost - accumulatedDepreciationAtDisposal;
            decimal disposalAmount = asset.DisposalAmount ?? 0m;
            decimal gainLossOnDisposal = disposalAmount - bookValueAtDisposal;

            var line = new AssetDisposalReportLineDto
            {
                FixedAssetId = asset.Id,
                AssetNumber = asset.AssetNumber,
                AssetName = asset.AssetName,
                AssetCategoryName = asset.AssetCategory?.Name ?? "N/A", // AssetCategory included by spec
                PurchaseDate = asset.PurchaseDate,
                OriginalCost = asset.PurchaseCost,
                DisposalDate = asset.DisposalDate.Value,
                DisposalReason = asset.DisposalReason,
                DisposalAmount = disposalAmount,
                AccumulatedDepreciationAtDisposal = accumulatedDepreciationAtDisposal,
                BookValueAtDisposal = bookValueAtDisposal,
                GainLossOnDisposal = gainLossOnDisposal
            };
            reportDto.Disposals.Add(line);

            reportDto.GrandTotalOriginalCost += line.OriginalCost;
            reportDto.GrandTotalAccumulatedDepreciationAtDisposal += line.AccumulatedDepreciationAtDisposal;
            reportDto.GrandTotalDisposalAmount += line.DisposalAmount;
            reportDto.GrandTotalGainLossOnDisposal += line.GainLossOnDisposal;
        }

        // Sorting is handled by the spec (DisposalDate ASC, AssetNumber ASC)
        // reportDto.Disposals = reportDto.Disposals.OrderBy(d => d.DisposalDate).ThenBy(d => d.AssetNumber).ToList();

        return reportDto;
    }
}
