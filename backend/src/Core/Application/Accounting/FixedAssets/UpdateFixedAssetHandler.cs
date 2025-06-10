using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class UpdateFixedAssetHandler : IRequestHandler<UpdateFixedAssetRequest, Guid>
{
    private readonly IRepository<FixedAsset> _assetRepository;
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod> _depreciationMethodRepository;
    private readonly IStringLocalizer<UpdateFixedAssetHandler> _localizer;
    private readonly ILogger<UpdateFixedAssetHandler> _logger;

    public UpdateFixedAssetHandler(
        IRepository<FixedAsset> assetRepository,
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<DepreciationMethod> depreciationMethodRepository,
        IStringLocalizer<UpdateFixedAssetHandler> localizer,
        ILogger<UpdateFixedAssetHandler> logger)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _depreciationMethodRepository = depreciationMethodRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _assetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (fixedAsset == null)
            throw new NotFoundException(_localizer["Fixed Asset with ID {0} not found.", request.Id]);

        if (fixedAsset.Status == FixedAssetStatus.Disposed)
            throw new ConflictException(_localizer["Cannot update a disposed Fixed Asset."]);

        // Validate AssetCategory if changed
        if (request.AssetCategoryId.HasValue && request.AssetCategoryId.Value != fixedAsset.AssetCategoryId)
        {
            var assetCategory = await _categoryRepository.GetByIdAsync(request.AssetCategoryId.Value, cancellationToken);
            if (assetCategory == null /* || !assetCategory.IsActive */)
                throw new NotFoundException(_localizer["Asset Category with ID {0} not found or is inactive.", request.AssetCategoryId.Value]);
        }

        // Validate DepreciationMethod if changed
        if (request.DepreciationMethodId.HasValue && request.DepreciationMethodId.Value != fixedAsset.DepreciationMethodId)
        {
            var depMethod = await _depreciationMethodRepository.GetByIdAsync(request.DepreciationMethodId.Value, cancellationToken);
            if (depMethod == null /* || !depMethod.IsActive */)
                throw new NotFoundException(_localizer["Depreciation Method with ID {0} not found or is inactive.", request.DepreciationMethodId.Value]);
        }

        // AssetNumber is not updated. If it were, uniqueness check would be needed:
        // if (request.AssetNumber is not null && !fixedAsset.AssetNumber.Equals(request.AssetNumber, StringComparison.OrdinalIgnoreCase))
        // {
        //     var existingAssetByNumber = await _assetRepository.FirstOrDefaultAsync(new FixedAssetByAssetNumberSpec(request.AssetNumber), cancellationToken);
        //     if (existingAssetByNumber != null && existingAssetByNumber.Id != fixedAsset.Id)
        //         throw new ConflictException(_localizer["Fixed Asset with number {0} already exists.", request.AssetNumber]);
        // }

        fixedAsset.Update(
            assetName: request.AssetName,
            description: request.Description,
            assetCategoryId: request.AssetCategoryId,
            purchaseDate: request.PurchaseDate,
            purchaseCost: request.PurchaseCost,
            salvageValue: request.SalvageValue,
            usefulLifeYears: request.UsefulLifeYears,
            depreciationMethodId: request.DepreciationMethodId,
            location: request.Location,
            status: request.Status
            // Disposal fields are handled by DisposeFixedAssetHandler
        );

        await _assetRepository.UpdateAsync(fixedAsset, cancellationToken);
        _logger.LogInformation(_localizer["Fixed Asset '{0}' (ID: {1}) updated."], fixedAsset.AssetName, fixedAsset.Id);
        return fixedAsset.Id;
    }
}
