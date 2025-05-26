namespace FSH.WebApi.Application.Catalog.Services;

public class SearchServicesRequest : PaginationFilter, IRequest<PaginationResponse<ServiceDto>>
{
}

public class ServiceBySearchRequestSpec : EntitiesByPaginationFilterSpec<Service, ServiceDto>
{
    public ServiceBySearchRequestSpec(SearchServicesRequest request)
        : base(request) =>
        Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchServiceRequestHandler : IRequestHandler<SearchServicesRequest,
PaginationResponse<ServiceDto>>
{
    private readonly IReadRepository<Service> _repository;

    public SearchServiceRequestHandler(IReadRepository<Service> repository) => _repository = repository;

    public async Task<PaginationResponse<ServiceDto>> Handle(SearchServicesRequest request, CancellationToken
    cancellationToken)
    {
        var spec = new ServiceBySearchRequestSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}