using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class CreateDepreciationMethodHandler : IRequestHandler<CreateDepreciationMethodRequest, Guid>
{
    private readonly IRepository<DepreciationMethod> _repository;
    private readonly IStringLocalizer<CreateDepreciationMethodHandler> _localizer;
    private readonly ILogger<CreateDepreciationMethodHandler> _logger;

    public CreateDepreciationMethodHandler(
        IRepository<DepreciationMethod> repository,
        IStringLocalizer<CreateDepreciationMethodHandler> localizer,
        ILogger<CreateDepreciationMethodHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateDepreciationMethodRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.FirstOrDefaultAsync(new DepreciationMethodByNameSpec(request.Name), cancellationToken);
        if (existing != null)
        {
            throw new ConflictException(_localizer["A depreciation method with this name already exists."]);
        }

        var depreciationMethod = new DepreciationMethod(
            name: request.Name,
            description: request.Description
        );

        await _repository.AddAsync(depreciationMethod, cancellationToken);
        _logger.LogInformation(_localizer["Depreciation Method '{0}' created."], depreciationMethod.Name);
        return depreciationMethod.Id;
    }
}
