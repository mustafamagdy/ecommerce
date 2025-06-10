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

public class UpdateDepreciationMethodHandler : IRequestHandler<UpdateDepreciationMethodRequest, Guid>
{
    private readonly IRepository<DepreciationMethod> _repository;
    private readonly IStringLocalizer<UpdateDepreciationMethodHandler> _localizer;
    private readonly ILogger<UpdateDepreciationMethodHandler> _logger;

    public UpdateDepreciationMethodHandler(
        IRepository<DepreciationMethod> repository,
        IStringLocalizer<UpdateDepreciationMethodHandler> localizer,
        ILogger<UpdateDepreciationMethodHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdateDepreciationMethodRequest request, CancellationToken cancellationToken)
    {
        var depreciationMethod = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (depreciationMethod == null)
        {
            throw new NotFoundException(_localizer["Depreciation Method with ID {0} not found.", request.Id]);
        }

        if (request.Name is not null && !depreciationMethod.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _repository.FirstOrDefaultAsync(new DepreciationMethodByNameSpec(request.Name), cancellationToken);
            if (existing != null && existing.Id != depreciationMethod.Id)
            {
                throw new ConflictException(_localizer["A depreciation method with this name already exists."]);
            }
        }

        depreciationMethod.Update(
            name: request.Name,
            description: request.Description
        );

        await _repository.UpdateAsync(depreciationMethod, cancellationToken);
        _logger.LogInformation(_localizer["Depreciation Method '{0}' (ID: {1}) updated."], depreciationMethod.Name, depreciationMethod.Id);
        return depreciationMethod.Id;
    }
}
