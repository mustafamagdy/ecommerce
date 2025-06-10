using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class DisposeFixedAssetHandler : IRequestHandler<DisposeFixedAssetRequest, Guid>
{
    private readonly IRepository<FixedAsset> _assetRepository;
    private readonly IStringLocalizer<DisposeFixedAssetHandler> _localizer;
    private readonly ILogger<DisposeFixedAssetHandler> _logger;

    public DisposeFixedAssetHandler(
        IRepository<FixedAsset> assetRepository,
        IStringLocalizer<DisposeFixedAssetHandler> localizer,
        ILogger<DisposeFixedAssetHandler> logger)
    {
        _assetRepository = assetRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DisposeFixedAssetRequest request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _assetRepository.GetByIdAsync(request.FixedAssetId, cancellationToken);
        if (fixedAsset == null)
            throw new NotFoundException(_localizer["Fixed Asset with ID {0} not found.", request.FixedAssetId]);

        if (fixedAsset.Status == FixedAssetStatus.Disposed)
            throw new ConflictException(_localizer["Fixed Asset {0} is already disposed.", fixedAsset.AssetNumber]);

        // Call the domain entity's Dispose method
        fixedAsset.Dispose(
            disposalDate: request.DisposalDate,
            reason: request.DisposalReason,
            amount: request.DisposalAmount
        );

        // The FixedAsset.Update method was also modified to accept disposal fields,
        // but calling the specific Dispose method on the entity is cleaner.
        // fixedAsset.Update(null,null,null,null,null,null,null,null,null, FixedAssetStatus.Disposed, request.DisposalDate, request.DisposalReason, request.DisposalAmount);


        await _assetRepository.UpdateAsync(fixedAsset, cancellationToken);
        _logger.LogInformation(_localizer["Fixed Asset '{0}' (Number: {1}) disposed."], fixedAsset.AssetName, fixedAsset.AssetNumber);
        return fixedAsset.Id;
    }
}
