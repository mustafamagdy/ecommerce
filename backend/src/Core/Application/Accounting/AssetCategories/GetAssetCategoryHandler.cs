using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class GetAssetCategoryHandler : IRequestHandler<GetAssetCategoryRequest, AssetCategoryDto>
{
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod>? _depreciationMethodRepository; // To fetch name
    private readonly IStringLocalizer<GetAssetCategoryHandler> _localizer;

    public GetAssetCategoryHandler(
        IReadRepository<AssetCategory> categoryRepository,
        IStringLocalizer<GetAssetCategoryHandler> localizer,
        IReadRepository<DepreciationMethod>? depreciationMethodRepository = null)
    {
        _categoryRepository = categoryRepository;
        _localizer = localizer;
        _depreciationMethodRepository = depreciationMethodRepository;
    }

    public async Task<AssetCategoryDto> Handle(GetAssetCategoryRequest request, CancellationToken cancellationToken)
    {
        // Using the AssetCategoryByIdWithDetailsSpec (even if it doesn't include nav props yet)
        // allows for future expansion of the spec without changing handler logic for includes.
        var spec = new AssetCategoryByIdWithDetailsSpec(request.Id);
        var assetCategory = await _categoryRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assetCategory == null)
        {
            throw new NotFoundException(_localizer["Asset Category with ID {0} not found.", request.Id]);
        }

        var dto = assetCategory.Adapt<AssetCategoryDto>();

        if (assetCategory.DefaultDepreciationMethodId.HasValue && _depreciationMethodRepository != null)
        {
            var depMethod = await _depreciationMethodRepository.GetByIdAsync(assetCategory.DefaultDepreciationMethodId.Value, cancellationToken);
            dto.DefaultDepreciationMethodName = depMethod?.Name;
        }
        // dto.IsActive = assetCategory.IsActive; // Already mapped by Adapt if DTO has IsActive

        return dto;
    }
}
