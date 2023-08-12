namespace FSH.WebApi.Application.Catalog.Brands;

public class SearchBrandsRequest : PaginationFilter, IRequest<PaginationResponse<BrandDto>>
{
}

public class BrandsBySearchRequestSpec : EntitiesByPaginationFilterSpec<Brand, BrandDto>
{
  public BrandsBySearchRequestSpec(SearchBrandsRequest request)
    : base(request) => Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchBrandsRequestHandler : IRequestHandler<SearchBrandsRequest, PaginationResponse<BrandDto>>
{
  private readonly IReadRepository<Brand> _repository;

  public SearchBrandsRequestHandler(IReadRepository<Brand> repository) => _repository = repository;

  public Task<PaginationResponse<BrandDto>> Handle(SearchBrandsRequest request, CancellationToken cancellationToken)
    => _repository.PaginatedListAsync(new BrandsBySearchRequestSpec(request), request.PageNumber, request.PageSize, cancellationToken);
}