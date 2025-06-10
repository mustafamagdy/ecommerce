using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class UpdateAssetCategoryHandler : IRequestHandler<UpdateAssetCategoryRequest, Guid>
{
    private readonly IRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod>? _depreciationMethodRepository; // To validate DefaultDepreciationMethodId
    private readonly IStringLocalizer<UpdateAssetCategoryHandler> _localizer;
    private readonly ILogger<UpdateAssetCategoryHandler> _logger;

    public UpdateAssetCategoryHandler(
        IRepository<AssetCategory> categoryRepository,
        IStringLocalizer<UpdateAssetCategoryHandler> localizer,
        ILogger<UpdateAssetCategoryHandler> logger,
        IReadRepository<DepreciationMethod>? depreciationMethodRepository = null)
    {
        _categoryRepository = categoryRepository;
        _localizer = localizer;
        _logger = logger;
        _depreciationMethodRepository = depreciationMethodRepository;
    }

    public async Task<Guid> Handle(UpdateAssetCategoryRequest request, CancellationToken cancellationToken)
    {
        var assetCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (assetCategory == null)
        {
            throw new NotFoundException(_localizer["Asset Category with ID {0} not found.", request.Id]);
        }

        if (request.Name is not null && !assetCategory.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingCategory = await _categoryRepository.FirstOrDefaultAsync(new AssetCategoryByNameSpec(request.Name), cancellationToken);
            if (existingCategory != null && existingCategory.Id != assetCategory.Id)
            {
                throw new ConflictException(_localizer["An asset category with this name already exists."]);
            }
        }

        if (request.DefaultDepreciationMethodId.HasValue &&
            request.DefaultDepreciationMethodId != assetCategory.DefaultDepreciationMethodId &&
            _depreciationMethodRepository != null)
        {
            var depMethod = await _depreciationMethodRepository.GetByIdAsync(request.DefaultDepreciationMethodId.Value, cancellationToken);
            if (depMethod == null)
            {
                throw new NotFoundException(_localizer["Default Depreciation Method with ID {0} not found.", request.DefaultDepreciationMethodId.Value]);
            }
        }

        assetCategory.Update(
            name: request.Name,
            description: request.Description,
            defaultDepreciationMethodId: request.DefaultDepreciationMethodId,
            defaultUsefulLifeYears: request.DefaultUsefulLifeYears,
            isActive: request.IsActive
        );

        await _categoryRepository.UpdateAsync(assetCategory, cancellationToken);
        _logger.LogInformation(_localizer["Asset Category '{0}' (ID: {1}) updated."], assetCategory.Name, assetCategory.Id);
        return assetCategory.Id;
    }
}
