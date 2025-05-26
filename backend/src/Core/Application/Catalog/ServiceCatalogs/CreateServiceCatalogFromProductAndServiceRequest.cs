using FluentValidation.Validators;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class CreateServiceCatalogFromProductAndServiceRequest : IRequest<Guid>
{
  public decimal Price { get; set; }
  public string CategoryName { get; set; }
  public string ProductName { get; set; }
  public string ServiceName { get; set; }
  public FileUploadRequest? ProductImage { get; set; }
}

public class
  CreateServiceCatalogFromProductAndServiceRequestValidator : CustomValidator<
    CreateServiceCatalogFromProductAndServiceRequest>
{
  private readonly IValidator<Product> _productValidator;

  public CreateServiceCatalogFromProductAndServiceRequestValidator(
    IReadRepository<Category> categoryRepo,
    IReadRepository<Product> prodRepo,
    IReadRepository<Service> serviceRepo,
    IStringLocalizer<CreateServiceCatalogFromProductAndServiceRequestValidator> t)
  {
    // Include(new CreateProductRequestValidator(prodRepo,brandRepo,tPro));
    RuleFor(a => a.Price).GreaterThanOrEqualTo(0);
    RuleFor(a => a.CategoryName).NotEmpty().MustAsync(async (name, cancellationToken) =>
    {
      var exist = await categoryRepo.AnyAsync(new SingleResultSpecification<Category>()
        .Query.Where(a => a.Name.ToLower() == name)
        .Specification, cancellationToken);
      return !exist;
    }).WithMessage(t["Product with same name already exist"]);

    RuleFor(a => a.ProductName).NotEmpty().MustAsync(async (name, cancellationToken) =>
    {
      var exist = await prodRepo.AnyAsync(new SingleResultSpecification<Product>()
        .Query.Where(a => a.Name.ToLower() == name)
        .Specification, cancellationToken);
      return !exist;
    }).WithMessage(t["Product with same name already exist"]);

    RuleFor(a => a.ServiceName).NotEmpty().MustAsync(async (name, cancellationToken) =>
    {
      var exist = await serviceRepo.AnyAsync(new SingleResultSpecification<Service>()
        .Query.Where(a => a.Name.ToLower() == name)
        .Specification, cancellationToken);
      return !exist;
    }).WithMessage(t["Service with same name already exist"]);
  }
}

public class
  CreateServiceCatalogFromProductAndServiceRequestHandler : IRequestHandler<
    CreateServiceCatalogFromProductAndServiceRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IRepositoryWithEvents<Category> _categoryRepo;
  private IRepositoryWithEvents<Product> _prodRepo;
  private IRepositoryWithEvents<Service> _serviceRepo;
  private readonly ISystemDefaults _systemDefaults;
  private readonly IFileStorageService _file;

  public CreateServiceCatalogFromProductAndServiceRequestHandler(
    IRepository<ServiceCatalog> repository,
    IApplicationUnitOfWork uow, IRepositoryWithEvents<Category> categoryRepo,
    IRepositoryWithEvents<Product> prodRepo, IRepositoryWithEvents<Service> serviceRepo,
    ISystemDefaults systemDefaults, IFileStorageService file)
  {
    _repository = repository;
    _uow = uow;
    _categoryRepo = categoryRepo;
    _prodRepo = prodRepo;
    _serviceRepo = serviceRepo;
    _systemDefaults = systemDefaults;
    _file = file;
  }

  public async Task<Guid> Handle(CreateServiceCatalogFromProductAndServiceRequest request,
    CancellationToken cancellationToken)
  {
    var brand = await _systemDefaults.GetDefaultBrandAsync(cancellationToken);

    string productImagePath = await _file.UploadAsync<Product>(request.ProductImage, FileType.Image, cancellationToken);

    var category = new Category(request.ProductName, request.ProductName);
    category.AddDomainEvent(EntityCreatedEvent.WithEntity(category));
    await _categoryRepo.AddAsync(category, cancellationToken);

    var product = new Product(request.ProductName, request.ProductName, request.Price, brand.Id, productImagePath);
    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));
    await _prodRepo.AddAsync(product, cancellationToken);

    var service = new Service(request.ServiceName, "", null);
    service.AddDomainEvent(EntityCreatedEvent.WithEntity(service));
    await _serviceRepo.AddAsync(service, cancellationToken);

    var catalogItem = new ServiceCatalog(service.Id, product.Id, category.Id, request.Price);
    catalogItem.AddDomainEvent(EntityCreatedEvent.WithEntity(catalogItem));
    await _repository.AddAsync(catalogItem, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return catalogItem.Id;
  }
}