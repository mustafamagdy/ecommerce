using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class SearchFixedAssetsHandler : IRequestHandler<SearchFixedAssetsRequest, PaginationResponse<FixedAssetDto>>
{
    private readonly IReadRepository<FixedAsset> _assetRepository;
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod> _depreciationMethodRepository;
    private readonly IStringLocalizer<SearchFixedAssetsHandler> _localizer;

    public SearchFixedAssetsHandler(
        IReadRepository<FixedAsset> assetRepository,
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<DepreciationMethod> depreciationMethodRepository,
        IStringLocalizer<SearchFixedAssetsHandler> localizer)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _depreciationMethodRepository = depreciationMethodRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<FixedAssetDto>> Handle(SearchFixedAssetsRequest request, CancellationToken cancellationToken)
    {
        var spec = new FixedAssetsBySearchFilterSpec(request);
        var assets = await _assetRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _assetRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<FixedAssetDto>();

        // For populating names, fetch related entities efficiently.
        // Get all unique Category IDs and Method IDs from the current page of assets
        var categoryIds = assets.Select(a => a.AssetCategoryId).Distinct().ToList();
        var methodIds = assets.Select(a => a.DepreciationMethodId).Distinct().ToList();

        var categories = categoryIds.Any()
            ? (await _categoryRepository.ListAsync(new AssetCategoriesByIdsSpec(categoryIds), cancellationToken))
                .ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var methods = methodIds.Any()
            ? (await _depreciationMethodRepository.ListAsync(new DepreciationMethodsByIdsSpec(methodIds), cancellationToken)) // Assumes DepreciationMethodsByIdsSpec exists
                .ToDictionary(m => m.Id, m => m.Name)
            : new Dictionary<Guid, string>();


        foreach (var asset in assets)
        {
            var dto = asset.Adapt<FixedAssetDto>();
            dto.Status = asset.Status.ToString();

            if (categories.TryGetValue(asset.AssetCategoryId, out var categoryName))
            {
                dto.AssetCategoryName = categoryName;
            }
            if (methods.TryGetValue(asset.DepreciationMethodId, out var methodName))
            {
                dto.DepreciationMethodName = methodName;
            }
            dtos.Add(dto);
        }

        return new PaginationResponse<FixedAssetDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// Helper Spec to fetch multiple AssetCategories by IDs (similar to DepreciationMethodsByIdsSpec)
public class AssetCategoriesByIdsSpec : Specification<AssetCategory>
{
    public AssetCategoriesByIdsSpec(List<Guid> ids)
    {
        Query.Where(ac => ids.Contains(ac.Id));
    }
}
