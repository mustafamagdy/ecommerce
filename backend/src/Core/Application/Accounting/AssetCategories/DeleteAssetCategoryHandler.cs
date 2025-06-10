using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class FixedAssetByAssetCategorySpec : Specification<FixedAsset>, ISingleResultSpecification
{
    public FixedAssetByAssetCategorySpec(Guid assetCategoryId) =>
        Query.Where(fa => fa.AssetCategoryId == assetCategoryId);
}

public class DeleteAssetCategoryHandler : IRequestHandler<DeleteAssetCategoryRequest, Guid>
{
    private readonly IRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<FixedAsset> _assetRepository;
    private readonly IStringLocalizer<DeleteAssetCategoryHandler> _localizer;
    private readonly ILogger<DeleteAssetCategoryHandler> _logger;

    public DeleteAssetCategoryHandler(
        IRepository<AssetCategory> categoryRepository,
        IReadRepository<FixedAsset> assetRepository,
        IStringLocalizer<DeleteAssetCategoryHandler> localizer,
        ILogger<DeleteAssetCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _assetRepository = assetRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteAssetCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
            throw new NotFoundException(_localizer["Asset Category with ID {0} not found.", request.Id]);

        bool categoryInUse = await _assetRepository.AnyAsync(new FixedAssetByAssetCategorySpec(request.Id), cancellationToken);
        if (categoryInUse)
            throw new ConflictException(_localizer["Asset Category '{0}' is in use by one or more fixed assets.", category.Name]);

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        _logger.LogInformation(_localizer["Asset Category '{0}' (ID: {1}) deleted."], category.Name, category.Id);
        return category.Id;
    }
}
