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

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class AssetCategoryByDepreciationMethodSpec : Specification<AssetCategory>, ISingleResultSpecification
{
    public AssetCategoryByDepreciationMethodSpec(Guid depreciationMethodId) =>
        Query.Where(ac => ac.DefaultDepreciationMethodId == depreciationMethodId);
}

public class FixedAssetByDepreciationMethodSpec : Specification<FixedAsset>, ISingleResultSpecification
{
    public FixedAssetByDepreciationMethodSpec(Guid depreciationMethodId) =>
        Query.Where(fa => fa.DepreciationMethodId == depreciationMethodId);
}

public class DeleteDepreciationMethodHandler : IRequestHandler<DeleteDepreciationMethodRequest, Guid>
{
    private readonly IRepository<DepreciationMethod> _methodRepository;
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<FixedAsset> _assetRepository;
    private readonly IStringLocalizer<DeleteDepreciationMethodHandler> _localizer;
    private readonly ILogger<DeleteDepreciationMethodHandler> _logger;

    public DeleteDepreciationMethodHandler(
        IRepository<DepreciationMethod> methodRepository,
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<FixedAsset> assetRepository,
        IStringLocalizer<DeleteDepreciationMethodHandler> localizer,
        ILogger<DeleteDepreciationMethodHandler> logger)
    {
        _methodRepository = methodRepository;
        _categoryRepository = categoryRepository;
        _assetRepository = assetRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteDepreciationMethodRequest request, CancellationToken cancellationToken)
    {
        var method = await _methodRepository.GetByIdAsync(request.Id, cancellationToken);
        if (method == null)
            throw new NotFoundException(_localizer["Depreciation Method with ID {0} not found.", request.Id]);

        bool inUseByCategory = await _categoryRepository.AnyAsync(new AssetCategoryByDepreciationMethodSpec(request.Id), cancellationToken);
        if (inUseByCategory)
            throw new ConflictException(_localizer["Depreciation Method '{0}' is in use by one or more asset categories.", method.Name]);

        bool inUseByAsset = await _assetRepository.AnyAsync(new FixedAssetByDepreciationMethodSpec(request.Id), cancellationToken);
        if (inUseByAsset)
            throw new ConflictException(_localizer["Depreciation Method '{0}' is in use by one or more fixed assets.", method.Name]);

        await _methodRepository.DeleteAsync(method, cancellationToken);
        _logger.LogInformation(_localizer["Depreciation Method '{0}' (ID: {1}) deleted."], method.Name, method.Id);
        return method.Id;
    }
}
