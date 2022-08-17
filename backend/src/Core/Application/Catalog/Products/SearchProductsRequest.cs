namespace FSH.WebApi.Application.Catalog.Products;

public class SearchProductsRequest : PaginationFilter, IRequest<PaginationResponse<ProductDto>>
{
  public Guid? BrandId { get; set; }
  public decimal? MinimumRate { get; set; }
  public decimal? MaximumRate { get; set; }
}

public class SearchProductsRequestHandler : IRequestHandler<SearchProductsRequest, PaginationResponse<ProductDto>>
{
  private readonly IReadRepository<Product> _repository;

  public SearchProductsRequestHandler(IReadRepository<Product> repository) => _repository = repository;

  public async Task<PaginationResponse<ProductDto>> Handle(SearchProductsRequest request, CancellationToken cancellationToken)
    => await _repository.PaginatedListAsync(
      new ProductsBySearchRequestWithBrandsSpec(request),
      request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
}