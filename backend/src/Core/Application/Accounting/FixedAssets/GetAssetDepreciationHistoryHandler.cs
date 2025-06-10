using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class GetAssetDepreciationHistoryHandler : IRequestHandler<GetAssetDepreciationHistoryRequest, PaginationResponse<AssetDepreciationEntryDto>>
{
    private readonly IReadRepository<AssetDepreciationEntry> _entryRepository;
    private readonly IReadRepository<FixedAsset> _assetRepository; // To verify FixedAssetId exists
    private readonly IStringLocalizer<GetAssetDepreciationHistoryHandler> _localizer;

    public GetAssetDepreciationHistoryHandler(
        IReadRepository<AssetDepreciationEntry> entryRepository,
        IReadRepository<FixedAsset> assetRepository,
        IStringLocalizer<GetAssetDepreciationHistoryHandler> localizer)
    {
        _entryRepository = entryRepository;
        _assetRepository = assetRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<AssetDepreciationEntryDto>> Handle(GetAssetDepreciationHistoryRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate FixedAssetId
        var fixedAsset = await _assetRepository.GetByIdAsync(request.FixedAssetId, cancellationToken);
        if (fixedAsset == null)
        {
            throw new Common.Exceptions.NotFoundException(_localizer["Fixed Asset with ID {0} not found.", request.FixedAssetId]);
        }

        // 2. Use the paginated specification
        var spec = new PaginatedAssetDepreciationHistorySpec(request);

        // 3. Fetch data using ListAsync for pagination-aware results
        var entries = await _entryRepository.ListAsync(spec, cancellationToken);
        // 4. Get total count using the same spec
        var totalCount = await _entryRepository.CountAsync(spec, cancellationToken);

        // 5. Adapt to DTOs
        var dtos = entries.Adapt<List<AssetDepreciationEntryDto>>();
        // Optionally, populate FixedAssetName on each DTO if it was added to AssetDepreciationEntryDto
        // foreach(var dto in dtos) { dto.FixedAssetName = fixedAsset.AssetName; }


        return new PaginationResponse<AssetDepreciationEntryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
