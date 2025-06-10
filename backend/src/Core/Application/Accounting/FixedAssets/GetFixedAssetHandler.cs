using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class GetFixedAssetHandler : IRequestHandler<GetFixedAssetRequest, FixedAssetDto>
{
    private readonly IReadRepository<FixedAsset> _assetRepository;
    private readonly IReadRepository<AssetCategory> _categoryRepository;
    private readonly IReadRepository<DepreciationMethod> _depreciationMethodRepository;
    private readonly IStringLocalizer<GetFixedAssetHandler> _localizer;

    public GetFixedAssetHandler(
        IReadRepository<FixedAsset> assetRepository,
        IReadRepository<AssetCategory> categoryRepository,
        IReadRepository<DepreciationMethod> depreciationMethodRepository,
        IStringLocalizer<GetFixedAssetHandler> localizer)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _depreciationMethodRepository = depreciationMethodRepository;
        _localizer = localizer;
    }

    public async Task<FixedAssetDto> Handle(GetFixedAssetRequest request, CancellationToken cancellationToken)
    {
        // Spec can be used here, but since we need to fetch related names anyway, direct GetByIdAsync is also fine.
        // Using the spec is good for consistency if it includes navigation properties.
        var spec = new FixedAssetByIdWithDetailsSpec(request.Id);
        var fixedAsset = await _assetRepository.FirstOrDefaultAsync(spec, cancellationToken); // Using FirstOrDefaultAsync with Spec

        if (fixedAsset == null)
            throw new NotFoundException(_localizer["Fixed Asset with ID {0} not found.", request.Id]);

        var dto = fixedAsset.Adapt<FixedAssetDto>();
        dto.Status = fixedAsset.Status.ToString(); // Map enum to string

        // Populate AssetCategoryName
        var assetCategory = await _categoryRepository.GetByIdAsync(fixedAsset.AssetCategoryId, cancellationToken);
        dto.AssetCategoryName = assetCategory?.Name;

        // Populate DepreciationMethodName
        var depMethod = await _depreciationMethodRepository.GetByIdAsync(fixedAsset.DepreciationMethodId, cancellationToken);
        dto.DepreciationMethodName = depMethod?.Name;

        return dto;
    }
}
