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

public class CreateAssetCategoryHandler : IRequestHandler<CreateAssetCategoryRequest, Guid>
{
    private readonly IRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod>? _depreciationMethodRepository; // To validate DefaultDepreciationMethodId
    private readonly IStringLocalizer<CreateAssetCategoryHandler> _localizer;
    private readonly ILogger<CreateAssetCategoryHandler> _logger;

    public CreateAssetCategoryHandler(
        IRepository<AssetCategory> categoryRepository,
        IStringLocalizer<CreateAssetCategoryHandler> localizer,
        ILogger<CreateAssetCategoryHandler> logger,
        IReadRepository<DepreciationMethod>? depreciationMethodRepository = null)
    {
        _categoryRepository = categoryRepository;
        _localizer = localizer;
        _logger = logger;
        _depreciationMethodRepository = depreciationMethodRepository;
    }

    public async Task<Guid> Handle(CreateAssetCategoryRequest request, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.FirstOrDefaultAsync(new AssetCategoryByNameSpec(request.Name), cancellationToken);
        if (existingCategory != null)
        {
            throw new ConflictException(_localizer["An asset category with this name already exists."]);
        }

        if (request.DefaultDepreciationMethodId.HasValue && _depreciationMethodRepository != null)
        {
            var depMethod = await _depreciationMethodRepository.GetByIdAsync(request.DefaultDepreciationMethodId.Value, cancellationToken);
            if (depMethod == null)
            {
                throw new NotFoundException(_localizer["Default Depreciation Method with ID {0} not found.", request.DefaultDepreciationMethodId.Value]);
            }
        }

        var assetCategory = new AssetCategory(
            name: request.Name,
            description: request.Description,
            defaultDepreciationMethodId: request.DefaultDepreciationMethodId,
            defaultUsefulLifeYears: request.DefaultUsefulLifeYears,
            isActive: request.IsActive
        );

        await _categoryRepository.AddAsync(assetCategory, cancellationToken);
        _logger.LogInformation(_localizer["Asset Category '{0}' created."], assetCategory.Name);
        return assetCategory.Id;
    }
}
