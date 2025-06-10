using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class GetDepreciationMethodHandler : IRequestHandler<GetDepreciationMethodRequest, DepreciationMethodDto>
{
    private readonly IReadRepository<DepreciationMethod> _repository;
    private readonly IStringLocalizer<GetDepreciationMethodHandler> _localizer;

    public GetDepreciationMethodHandler(IReadRepository<DepreciationMethod> repository, IStringLocalizer<GetDepreciationMethodHandler> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<DepreciationMethodDto> Handle(GetDepreciationMethodRequest request, CancellationToken cancellationToken)
    {
        var depreciationMethod = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (depreciationMethod == null)
        {
            throw new NotFoundException(_localizer["Depreciation Method with ID {0} not found.", request.Id]);
        }
        return depreciationMethod.Adapt<DepreciationMethodDto>();
    }
}
