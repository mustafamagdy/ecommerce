using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class SearchAssetCategoriesHandler : IRequestHandler<SearchAssetCategoriesRequest, PaginationResponse<AssetCategoryDto>>
{
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod>? _depreciationMethodRepository; // For populating names in list

    public SearchAssetCategoriesHandler(
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<DepreciationMethod>? depreciationMethodRepository = null)
    {
        _categoryRepository = categoryRepository;
        _depreciationMethodRepository = depreciationMethodRepository;
    }

    public async Task<PaginationResponse<AssetCategoryDto>> Handle(SearchAssetCategoriesRequest request, CancellationToken cancellationToken)
    {
        var spec = new AssetCategoriesBySearchFilterSpec(request);
        var categories = await _categoryRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _categoryRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<AssetCategoryDto>();
        if (_depreciationMethodRepository != null)
        {
            // Fetch all relevant depreciation methods in one go to avoid N+1 queries
            var methodIds = categories
                .Where(c => c.DefaultDepreciationMethodId.HasValue)
                .Select(c => c.DefaultDepreciationMethodId!.Value)
                .Distinct()
                .ToList();

            var depreciationMethods = methodIds.Any()
                ? (await _depreciationMethodRepository.ListAsync(new DepreciationMethodsByIdsSpec(methodIds), cancellationToken))
                    .ToDictionary(dm => dm.Id, dm => dm.Name)
                : new Dictionary<Guid, string>();

            foreach (var category in categories)
            {
                var dto = category.Adapt<AssetCategoryDto>();
                if (category.DefaultDepreciationMethodId.HasValue)
                {
                    depreciationMethods.TryGetValue(category.DefaultDepreciationMethodId.Value, out var methodName);
                    dto.DefaultDepreciationMethodName = methodName;
                }
                // dto.IsActive = category.IsActive; // Mapped by Adapt
                dtos.Add(dto);
            }
        }
        else // Fallback if no depreciation method repo, names will be null
        {
            dtos = categories.Adapt<List<AssetCategoryDto>>();
            // foreach(var dto in dtos) { dto.IsActive = categories.First(c=>c.Id == dto.Id).IsActive; }
        }


        return new PaginationResponse<AssetCategoryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// Helper spec to fetch multiple DepreciationMethods by IDs
public class DepreciationMethodsByIdsSpec : Specification<DepreciationMethod>
{
    public DepreciationMethodsByIdsSpec(List<Guid> ids)
    {
        Query.Where(dm => ids.Contains(dm.Id));
    }
}
