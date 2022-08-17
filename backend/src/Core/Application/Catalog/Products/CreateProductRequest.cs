using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Products;

public class CreateProductRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public decimal Rate { get; set; }
  public Guid BrandId { get; set; }
  public FileUploadRequest? Image { get; set; }
}

public class ProductByNameSpec : Specification<Product>, ISingleResultSpecification
{
  public ProductByNameSpec(string name) =>
    Query.Where(p => p.Name == name);
}
public class CreateProductRequestValidator : CustomValidator<CreateProductRequest>
{
  public CreateProductRequestValidator(IReadRepository<Product> productRepo, IReadRepository<Brand> brandRepo, IStringLocalizer<CreateProductRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await productRepo.FirstOrDefaultAsync(new ProductByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["Product {0} already Exists.", name]);

    RuleFor(p => p.Rate)
      .GreaterThanOrEqualTo(1);

    RuleFor(p => p.Image)
      .InjectValidator();

    RuleFor(p => p.BrandId)
      .NotEmpty()
      .MustAsync(async (id, ct) => await brandRepo.GetByIdAsync(id, ct) is not null)
      .WithMessage((_, id) => T["Brand {0} Not Found.", id]);
  }
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, Guid>
{
  private readonly IRepository<Product> _repository;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;

  public CreateProductRequestHandler(IRepository<Product> repository, IFileStorageService file, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateProductRequest request, CancellationToken cancellationToken)
  {
    string productImagePath = await _file.UploadAsync<Product>(request.Image, FileType.Image, cancellationToken);

    var product = new Product(request.Name, request.Description, request.Rate, request.BrandId, productImagePath);

    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));

    await _repository.AddAsync(product, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return product.Id;
  }
}