namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class SearchServiceCategoriesRequest : PaginationFilter, IRequest<PaginationResponse<ServiceCategoryDto>>
{
}

public class ServiceCategoriesBySearchRequestSpec : EntitiesByPaginationFilterSpec<Service, ServiceCategoryDto>
{
    public ServiceCategoriesBySearchRequestSpec(SearchServiceCategoriesRequest request)
        : base(request) =>
        Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchServiceCategoriesRequestHandler : IRequestHandler<SearchServiceCategoriesRequest,
PaginationResponse<ServiceCategoryDto>>
{
    private readonly IReadRepository<Service> _repository;

    public SearchServiceCategoriesRequestHandler(IReadRepository<Service> repository) => _repository = repository;

    public async Task<PaginationResponse<ServiceCategoryDto>> Handle(SearchServiceCategoriesRequest request, CancellationToken
    cancellationToken)
    {
        var spec = new ServiceCategoriesBySearchRequestSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}