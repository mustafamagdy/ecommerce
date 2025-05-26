namespace FSH.WebApi.Application.Catalog.Categories;

public class GetCategoryRequest : IRequest<CategoryDetailsDto>
{
  public Guid Id { get; set; }

  public GetCategoryRequest(Guid id) => Id = id;
}

public class CategoryByIdWithBrandSpec : Specification<Category, CategoryDetailsDto>, ISingleResultSpecification
{
  public CategoryByIdWithBrandSpec(Guid id) =>
    Query
      .Where(p => p.Id == id);
}

public class GetCategoryRequestHandler : IRequestHandler<GetCategoryRequest, CategoryDetailsDto>
{
  private readonly IRepository<Category> _repository;
  private readonly IStringLocalizer _t;

  public GetCategoryRequestHandler(
    IRepository<Category> repository,
    IStringLocalizer<GetCategoryRequestHandler> localizer) =>
    (_repository, _t) = (repository, localizer);

  public async Task<CategoryDetailsDto> Handle(GetCategoryRequest request, CancellationToken cancellationToken) =>
    await _repository.FirstOrDefaultAsync(
      new CategoryByIdWithBrandSpec(request.Id), cancellationToken)
    ?? throw new NotFoundException(_t["Category {0} Not Found.", request.Id]);
}