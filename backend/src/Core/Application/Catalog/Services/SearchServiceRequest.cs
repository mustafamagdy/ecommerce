namespace FSH.WebApi.Application.Catalog.Services;

public class SearchServiceRequest : PaginationFilter, IRequest<PaginationResponse<ServiceDto>>
{
}

public class ServiceBySearchRequestSpec : EntitiesByPaginationFilterSpec<Service, ServiceDto>
{
    public ServiceBySearchRequestSpec(SearchServiceRequest request)
        : base(request) =>
        Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchServiceRequestHandler : IRequestHandler<SearchServiceRequest,
PaginationResponse<ServiceDto>>
{
    private readonly IReadRepository<Service> _repository;

    public SearchServiceRequestHandler(IReadRepository<Service> repository) => _repository = repository;

    public async Task<PaginationResponse<ServiceDto>> Handle(SearchServiceRequest request, CancellationToken
    cancellationToken)
    {
        var spec = new ServiceBySearchRequestSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}