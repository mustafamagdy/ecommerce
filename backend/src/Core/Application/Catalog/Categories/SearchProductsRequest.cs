namespace FSH.WebApi.Application.Catalog.Categories;

public class SearchCategoriesRequest : PaginationFilter, IRequest<PaginationResponse<CategoryDto>>
{
}

public class SearchCategoriesRequestHandler : IRequestHandler<SearchCategoriesRequest, PaginationResponse<CategoryDto>>
{
  private readonly IReadRepository<Category> _repository;

  public SearchCategoriesRequestHandler(IReadRepository<Category> repository) => _repository = repository;

  public async Task<PaginationResponse<CategoryDto>> Handle(SearchCategoriesRequest request,
    CancellationToken cancellationToken)
    => await _repository.PaginatedListAsync(
      new CategoriesBySearchRequestSpec(request),
      request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
}

public class CategoriesBySearchRequestSpec : EntitiesByPaginationFilterSpec<Category, CategoryDto>
{
  public CategoriesBySearchRequestSpec(SearchCategoriesRequest request)
    : base(request) =>
    Query
      .OrderBy(c => c.Name, !request.HasOrderBy());
}