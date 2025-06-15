using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.FixedAssets.Specifications; // For the new spec
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class DepreciationScheduleReportHandler : IRequestHandler<DepreciationScheduleReportRequest, DepreciationScheduleReportDto>
{
    private readonly IReadRepository<FixedAsset> _assetRepo;
    private readonly IReadRepository<AssetCategory> _categoryRepo;
    // DepreciationMethod details are included in FixedAsset via spec.
    // AssetDepreciationEntry details are also included in FixedAsset via spec (and filtered).
    // private readonly IStringLocalizer<DepreciationScheduleReportHandler> _localizer;

    public DepreciationScheduleReportHandler(
        IReadRepository<FixedAsset> assetRepo,
        IReadRepository<AssetCategory> categoryRepo
        /* IStringLocalizer<DepreciationScheduleReportHandler> localizer */)
    {
        _assetRepo = assetRepo;
        _categoryRepo = categoryRepo;
        // _localizer = localizer;
    }

    public async Task<DepreciationScheduleReportDto> Handle(DepreciationScheduleReportRequest request, CancellationToken cancellationToken)
    {
        if (request.PeriodStartDate > request.PeriodEndDate)
        {
            // Or throw ValidationException if FluentValidation is not used for this request DTO.
            throw new ArgumentException("Period Start Date cannot be after Period End Date.");
        }

        var reportDto = new DepreciationScheduleReportDto
        {
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            AssetCategoryId = request.AssetCategoryId,
            FixedAssetId = request.FixedAssetId,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Lines = new List<DepreciationScheduleReportLineDto>()
        };

        if (request.AssetCategoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(request.AssetCategoryId.Value, cancellationToken);
            // if (category == null) throw new NotFoundException($"Asset Category with ID {request.AssetCategoryId.Value} not found.");
            reportDto.AssetCategoryName = category?.Name; // Allow null if not found, or throw as above
        }
        // FixedAssetName will be populated per line if FixedAssetId filter is not applied,
        // or set once here if it is.
        if (request.FixedAssetId.HasValue)
        {
            var tempAsset = await _assetRepo.GetByIdAsync(request.FixedAssetId.Value, cancellationToken);
            reportDto.FixedAssetName = tempAsset?.AssetName;
        }


        // 1. Fetch Fixed Assets with relevant, pre-filtered depreciation entries
        var assetsSpec = new FixedAssetsForDepreciationScheduleSpec(
            request.PeriodStartDate, request.PeriodEndDate, request.AssetCategoryId, request.FixedAssetId);
        var fixedAssets = await _assetRepo.ListAsync(assetsSpec, cancellationToken);

        if (!fixedAssets.Any())
        {
            return reportDto; // Return empty report
        }

        // 3. Populate Report Lines
        reportDto.GrandTotalDepreciationForPeriod = 0m;
        reportDto.GrandTotalPurchaseCost = 0m;
        reportDto.GrandTotalAccumulatedDepreciationAtPeriodEnd = 0m;
        reportDto.GrandTotalBookValueAtPeriodEnd = 0m;


        foreach (var asset in fixedAssets)
        {
            // All entries fetched are <= PeriodEndDate due to spec.
            var allEntriesForAsset = asset.DepreciationEntries.OrderBy(e => e.DepreciationDate).ToList();

            decimal accumulatedDepAtPeriodStart = allEntriesForAsset
                .Where(e => e.DepreciationDate < request.PeriodStartDate)
                .Sum(e => e.Amount);

            // Cap at depreciable base
            decimal depreciableBase = asset.PurchaseCost - asset.SalvageValue;
            if (accumulatedDepAtPeriodStart > depreciableBase && depreciableBase >=0) accumulatedDepAtPeriodStart = depreciableBase;
            if (accumulatedDepAtPeriodStart < 0) accumulatedDepAtPeriodStart = 0;


            decimal depreciationForPeriod = allEntriesForAsset
                .Where(e => e.DepreciationDate >= request.PeriodStartDate && e.DepreciationDate <= request.PeriodEndDate)
                .Sum(e => e.Amount);

            // Ensure depreciation for period doesn't cause total accumulated to exceed depreciable base
            if (accumulatedDepAtPeriodStart + depreciationForPeriod > depreciableBase && depreciableBase >=0)
            {
                depreciationForPeriod = Math.Max(0, depreciableBase - accumulatedDepAtPeriodStart);
            }
             if (depreciationForPeriod < 0) depreciationForPeriod = 0;


            decimal accumulatedDepAtPeriodEnd = accumulatedDepAtPeriodStart + depreciationForPeriod;
            if (accumulatedDepAtPeriodEnd > depreciableBase && depreciableBase >=0) accumulatedDepAtPeriodEnd = depreciableBase;
            if (accumulatedDepAtPeriodEnd < 0) accumulatedDepAtPeriodEnd = 0;


            decimal bookValueAtPeriodStart = asset.PurchaseCost - accumulatedDepAtPeriodStart;
            decimal bookValueAtPeriodEnd = asset.PurchaseCost - accumulatedDepAtPeriodEnd;


            var line = new DepreciationScheduleReportLineDto
            {
                FixedAssetId = asset.Id,
                AssetNumber = asset.AssetNumber,
                AssetName = asset.AssetName,
                AssetCategoryName = asset.AssetCategory?.Name ?? "N/A",
                PurchaseDate = asset.PurchaseDate,
                PurchaseCost = asset.PurchaseCost,
                SalvageValue = asset.SalvageValue,
                DepreciationMethodName = asset.DepreciationMethod?.Name ?? "N/A",
                UsefulLifeYears = asset.UsefulLifeYears,

                AccumulatedDepreciationAtPeriodStart = accumulatedDepAtPeriodStart,
                BookValueAtPeriodStart = bookValueAtPeriodStart,
                DepreciationAmountForPeriod = depreciationForPeriod,
                AccumulatedDepreciationAtPeriodEnd = accumulatedDepAtPeriodEnd,
                BookValueAtPeriodEnd = bookValueAtPeriodEnd
            };
            reportDto.Lines.Add(line);

            reportDto.GrandTotalDepreciationForPeriod += depreciationForPeriod;
            reportDto.GrandTotalPurchaseCost += asset.PurchaseCost; // Summing all assets' cost
            reportDto.GrandTotalAccumulatedDepreciationAtPeriodEnd += accumulatedDepAtPeriodEnd;
            reportDto.GrandTotalBookValueAtPeriodEnd += bookValueAtPeriodEnd;
        }

        // Sorting already handled by spec (AssetNumber ASC)
        // reportDto.Lines = reportDto.Lines.OrderBy(a => a.AssetNumber).ToList();

        return reportDto;
    }
}
