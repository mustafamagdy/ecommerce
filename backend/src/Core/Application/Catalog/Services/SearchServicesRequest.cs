namespace FSH.WebApi.Application.Catalog.Services;

public class SearchServicesRequest : PaginationFilter, IRequest<PaginationResponse<ServiceDto>>
{
  public Guid? ServiceCategoryId { get; set; }
}

public class SearchServicesRequestHandler : IRequestHandler<SearchServicesRequest, PaginationResponse<ServiceDto>>
{
  private readonly IReadRepository<Service> _repository;

  public SearchServicesRequestHandler(IReadRepository<Service> repository) => _repository = repository;

  public async Task<PaginationResponse<ServiceDto>> Handle(SearchServicesRequest request, CancellationToken
    cancellationToken)
  {
    var spec = new ServicesBySearchRequestWithServiceCategoriesSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
  }
}