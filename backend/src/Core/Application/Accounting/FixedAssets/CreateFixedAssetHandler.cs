using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

// Ad-hoc spec for number generation, similar to CreateCustomerInvoiceHandler
public class FixedAssetByNumberPrefixSpec : Specification<FixedAsset>
{
    public FixedAssetByNumberPrefixSpec(string prefix)
    {
        Query.Where(fa => fa.AssetNumber.StartsWith(prefix));
    }
}

public class CreateFixedAssetHandler : IRequestHandler<CreateFixedAssetRequest, Guid>
{
    private readonly IRepository<FixedAsset> _assetRepository;
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod> _depreciationMethodRepository;
    private readonly IStringLocalizer<CreateFixedAssetHandler> _localizer;
    private readonly ILogger<CreateFixedAssetHandler> _logger;

    public CreateFixedAssetHandler(
        IRepository<FixedAsset> assetRepository,
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<DepreciationMethod> depreciationMethodRepository,
        IStringLocalizer<CreateFixedAssetHandler> localizer,
        ILogger<CreateFixedAssetHandler> logger)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _depreciationMethodRepository = depreciationMethodRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateFixedAssetRequest request, CancellationToken cancellationToken)
    {
        // Validate AssetCategory
        var assetCategory = await _categoryRepository.GetByIdAsync(request.AssetCategoryId, cancellationToken);
        if (assetCategory == null /* || !assetCategory.IsActive */) // Assuming IsActive check is desired
            throw new NotFoundException(_localizer["Asset Category with ID {0} not found or is inactive.", request.AssetCategoryId]);

        // Validate DepreciationMethod
        var depMethod = await _depreciationMethodRepository.GetByIdAsync(request.DepreciationMethodId, cancellationToken);
        if (depMethod == null /* || !depMethod.IsActive */) // Assuming IsActive check is desired
            throw new NotFoundException(_localizer["Depreciation Method with ID {0} not found or is inactive.", request.DepreciationMethodId]);

        // Validate AssetNumber uniqueness if it's user-provided and meant to be unique
        if (!string.IsNullOrEmpty(request.AssetNumber))
        {
            var existingAssetByNumber = await _assetRepository.FirstOrDefaultAsync(new FixedAssetByAssetNumberSpec(request.AssetNumber), cancellationToken);
            if (existingAssetByNumber != null)
                throw new ConflictException(_localizer["Fixed Asset with number {0} already exists.", request.AssetNumber]);
        }
        else
        {
            // Auto-generate AssetNumber if not provided (or always auto-generate)
            // For this example, assuming request.AssetNumber should be validated for uniqueness if provided.
            // If always auto-generated, remove AssetNumber from request and generate here.
            // request.AssetNumber = await GenerateNextAssetNumberAsync(cancellationToken);
             throw new ValidationException(_localizer["Asset Number is required."]); // Or implement generation
        }


        var fixedAsset = new FixedAsset(
            assetNumber: request.AssetNumber,
            assetName: request.AssetName,
            assetCategoryId: request.AssetCategoryId,
            purchaseDate: request.PurchaseDate,
            purchaseCost: request.PurchaseCost,
            salvageValue: request.SalvageValue,
            usefulLifeYears: request.UsefulLifeYears,
            depreciationMethodId: request.DepreciationMethodId,
            description: request.Description,
            location: request.Location,
            status: FixedAssetStatus.Active // Default status on creation
        );

        await _assetRepository.AddAsync(fixedAsset, cancellationToken);
        _logger.LogInformation(_localizer["Fixed Asset '{0}' (Number: {1}) created."], fixedAsset.AssetName, fixedAsset.AssetNumber);
        return fixedAsset.Id;
    }

    // Example: private async Task<string> GenerateNextAssetNumberAsync(CancellationToken cancellationToken) { ... }
    // Similar to CustomerInvoice or CreditMemo number generation, with its caveats.
    // For now, assuming AssetNumber is provided in request and validated for uniqueness.
}
