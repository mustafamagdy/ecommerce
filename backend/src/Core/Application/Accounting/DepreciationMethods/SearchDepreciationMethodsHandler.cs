using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class SearchDepreciationMethodsHandler : IRequestHandler<SearchDepreciationMethodsRequest, PaginationResponse<DepreciationMethodDto>>
{
    private readonly IReadRepository<DepreciationMethod> _repository;

    public SearchDepreciationMethodsHandler(IReadRepository<DepreciationMethod> repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<DepreciationMethodDto>> Handle(SearchDepreciationMethodsRequest request, CancellationToken cancellationToken)
    {
        var spec = new DepreciationMethodsBySearchFilterSpec(request);
        var methods = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);
        var dtos = methods.Adapt<List<DepreciationMethodDto>>();

        return new PaginationResponse<DepreciationMethodDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
