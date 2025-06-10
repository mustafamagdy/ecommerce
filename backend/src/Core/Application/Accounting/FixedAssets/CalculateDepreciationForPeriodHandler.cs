using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification; // For Specification
using FSH.WebApi.Application.Common.Interfaces; // For IUnitOfWork (recommended)

namespace FSH.WebApi.Application.Accounting.FixedAssets;

// Spec to fetch assets eligible for depreciation
public class EligibleAssetsForDepreciationSpec : Specification<FixedAsset>
{
    public EligibleAssetsForDepreciationSpec(DateTime periodEndDate)
    {
        Query
            .Where(fa => fa.Status == FixedAssetStatus.Active || fa.Status == FixedAssetStatus.InRepair)
            .Where(fa => fa.PurchaseDate <= periodEndDate)
            // Asset is not fully depreciated: PurchaseCost - AccumulatedDepreciation > SalvageValue
            // This check is also inside FixedAsset.CalculateDepreciationForPeriod, but good to filter upfront.
            // However, direct translation of fa.BookValue > fa.SalvageValue might not work in EF Core if BookValue is not mapped.
            // So, we rely on the domain method's internal checks primarily, or expand this spec if needed.
            // For now, this simpler spec fetches active assets purchased on or before the period end.
            .Include(fa => fa.DepreciationEntries) // Needed to check last depreciation date and for updates
            .Include(fa => fa.DepreciationMethod); // Potentially needed by CalculateDepreciationForPeriod if it uses the nav prop
    }
}

public class CalculateDepreciationForPeriodHandler : IRequestHandler<CalculateDepreciationForPeriodRequest, CalculateDepreciationResult>
{
    private readonly IRepository<FixedAsset> _assetRepository;
    // private readonly IRepository<AssetDepreciationEntry> _entryRepository; // If entries were saved separately
    private readonly IStringLocalizer<CalculateDepreciationForPeriodHandler> _localizer;
    private readonly ILogger<CalculateDepreciationForPeriodHandler> _logger;
    // private readonly IUnitOfWork _unitOfWork; // Recommended

    public CalculateDepreciationForPeriodHandler(
        IRepository<FixedAsset> assetRepository,
        IStringLocalizer<CalculateDepreciationForPeriodHandler> localizer,
        ILogger<CalculateDepreciationForPeriodHandler> logger
        /* IRepository<AssetDepreciationEntry> entryRepository, IUnitOfWork unitOfWork */)
    {
        _assetRepository = assetRepository;
        _localizer = localizer;
        _logger = logger;
        // _entryRepository = entryRepository;
        // _unitOfWork = unitOfWork;
    }

    public async Task<CalculateDepreciationResult> Handle(CalculateDepreciationForPeriodRequest request, CancellationToken cancellationToken)
    {
        var result = new CalculateDepreciationResult();
        List<FixedAsset> assetsToProcess;

        // await _unitOfWork.BeginTransactionAsync(cancellationToken); // Example

        try
        {
            if (request.FixedAssetId.HasValue)
            {
                // Use a spec that also includes DepreciationEntries and DepreciationMethod
                var singleAssetSpec = new Specification<FixedAsset>();
                singleAssetSpec.Query
                    .Where(fa => fa.Id == request.FixedAssetId.Value)
                    .Include(fa => fa.DepreciationEntries)
                    .Include(fa => fa.DepreciationMethod);

                var asset = await _assetRepository.FirstOrDefaultAsync(singleAssetSpec, cancellationToken);
                if (asset == null)
                    throw new NotFoundException(_localizer["Fixed Asset with ID {0} not found.", request.FixedAssetId.Value]);
                assetsToProcess = new List<FixedAsset> { asset };
            }
            else
            {
                assetsToProcess = await _assetRepository.ListAsync(new EligibleAssetsForDepreciationSpec(request.PeriodEndDate), cancellationToken);
            }

            if (!assetsToProcess.Any())
            {
                result.Message = _localizer["No assets found eligible for depreciation for the period ending {0}.", request.PeriodEndDate.ToShortDateString()];
                return result;
            }

            foreach (var asset in assetsToProcess)
            {
                result.AssetsProcessed++;
                // The CalculateDepreciationForPeriod method on the entity handles logic and adds the entry to its own collection.
                var newEntry = asset.CalculateDepreciationForPeriod(request.PeriodEndDate);

                if (newEntry != null)
                {
                    // If successful, the asset's DepreciationEntries collection is modified.
                    // We need to persist the FixedAsset aggregate root.
                    await _assetRepository.UpdateAsync(asset, cancellationToken);
                    // If AssetDepreciationEntry was a separate aggregate and not persisted via FixedAsset:
                    // await _entryRepository.AddAsync(newEntry, cancellationToken);

                    result.DepreciationEntriesCreated++;
                    result.TotalDepreciationAmount += newEntry.Amount;
                    _logger.LogInformation(_localizer["Depreciation calculated for asset {0} (ID: {1}) for period ending {2}. Amount: {3}"],
                        asset.AssetName, asset.Id, request.PeriodEndDate.ToShortDateString(), newEntry.Amount);
                }
                else
                {
                    _logger.LogInformation(_localizer["Depreciation not applicable or already calculated for asset {0} (ID: {1}) for period ending {2}."],
                        asset.AssetName, asset.Id, request.PeriodEndDate.ToShortDateString());
                }
            }

            // await _unitOfWork.CommitTransactionAsync(cancellationToken); // Example

            result.Message = _localizer["Depreciation calculation completed. {0} assets processed, {1} new depreciation entries created. Total depreciation: {2}",
                result.AssetsProcessed, result.DepreciationEntriesCreated, result.TotalDepreciationAmount];

            return result;
        }
        catch (Exception ex)
        {
            // await _unitOfWork.RollbackTransactionAsync(cancellationToken); // Example
            _logger.LogError(ex, _localizer["Error during depreciation calculation."]);
            // Populate result with error message or rethrow depending on desired error handling strategy
            result.Message = _localizer["An error occurred during depreciation calculation: {0}", ex.Message];
            // For now, rethrowing to indicate failure, but could return result with error message.
            throw;
        }
    }
}
