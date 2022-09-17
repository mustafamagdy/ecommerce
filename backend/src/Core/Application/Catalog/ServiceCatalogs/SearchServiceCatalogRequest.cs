namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class SearchServiceCatalogRequest : PaginationFilter, IRequest<PaginationResponse<ServiceCatalogDto>>
{
}

public class SearchServiceCatalogRequestSpec : EntitiesByPaginationFilterSpec<ServiceCatalog, ServiceCatalogDto>
{
  public SearchServiceCatalogRequestSpec(SearchServiceCatalogRequest request)
    : base(request) => Query.Include(a => a.Service).Include(a => a.Product).OrderBy(c => c.Service.Name, !request.HasOrderBy());
}

public class SearchServiceCatalogRequestHandler : IRequestHandler<SearchServiceCatalogRequest,
  PaginationResponse<ServiceCatalogDto>>
{
  private readonly IReadRepository<ServiceCatalog> _repository;

  public SearchServiceCatalogRequestHandler(IReadRepository<ServiceCatalog> repository) => _repository = repository;

  public async Task<PaginationResponse<ServiceCatalogDto>> Handle(SearchServiceCatalogRequest request, CancellationToken
    cancellationToken)
    => await _repository.PaginatedListAsync(new SearchServiceCatalogRequestSpec(request),
      request.PageNumber, request.PageSize, cancellationToken);
}