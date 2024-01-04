using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Categories;

public class CreateCategoryRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public class CategoryByNameSpec : Specification<Category>, ISingleResultSpecification
{
  public CategoryByNameSpec(string name) =>
    Query.Where(p => p.Name == name);
}

public class CreateCategoryRequestValidator : CustomValidator<CreateCategoryRequest>
{
  public CreateCategoryRequestValidator(IReadRepository<Category> categoryRepo, IStringLocalizer<CreateCategoryRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await categoryRepo.FirstOrDefaultAsync(new CategoryByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["Category {0} already Exists.", name]);
  }
}

public class CreateCategoryRequestHandler : IRequestHandler<CreateCategoryRequest, Guid>
{
  private readonly IRepository<Category> _repository;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IReadRepository<Brand> _brandRepo;

  public CreateCategoryRequestHandler(IRepository<Category> repository, IFileStorageService file, IApplicationUnitOfWork uow,
    IReadRepository<Brand> brandRepo)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
    _brandRepo = brandRepo;
  }

  public async Task<Guid> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
  {
    var category = new Category(request.Name, request.Description);

    category.AddDomainEvent(EntityCreatedEvent.WithEntity(category));

    await _repository.AddAsync(category, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return category.Id;
  }
}