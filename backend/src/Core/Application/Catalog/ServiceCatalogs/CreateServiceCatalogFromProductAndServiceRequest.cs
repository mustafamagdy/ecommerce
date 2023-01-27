using FluentValidation.Validators;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class CreateServiceCatalogFromProductAndServiceRequest : IRequest<Guid>
{
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
  public Guid ServiceId{ get; set; }
  public CreateProductRequest Product { get; set; }
}

public class CreateServiceCatalogFromProductAndServiceRequestValidator : CustomValidator<CreateServiceCatalogFromProductAndServiceRequest>
{
    private readonly IValidator<Product> _productValidator;

    public CreateServiceCatalogFromProductAndServiceRequestValidator(
    IReadRepository<Product> prodRepo,
    IReadRepository<Brand> brandRepo,
    IReadRepository<Service> serviceRepo,
    IStringLocalizer<CreateProductRequestValidator> tProduct,
    IStringLocalizer<CreateServiceCatalogFromProductAndServiceRequestValidator> t)
  {
      // Include(new CreateProductRequestValidator(prodRepo,brandRepo,tPro));
    RuleFor(a => a.Price).GreaterThanOrEqualTo(0);
    RuleFor(a => a.Product).SetValidator(new CreateProductRequestValidator(prodRepo, brandRepo, tProduct));
    // RuleFor(a => a.ProductName).NotEmpty().MustAsync(async (name, cancellationToken) =>
    // {
    //   var exist = await prodRepo.AnyAsync(new SingleResultSpecification<Product>()
    //     .Query.Where(a => a.Name.ToLower() == name)
    //     .Specification, cancellationToken);
    //   return !exist;
    // }).WithMessage(t["Product with same name already exist"]);

    RuleFor(a => a.ServiceId).NotEmpty().MustAsync(async (id, cancellationToken) =>
    {
      var exist = await serviceRepo.AnyAsync(new SingleResultSpecification<Service>()
        .Query.Where(a => a.Id== id)
        .Specification, cancellationToken);
      return exist;
    }).WithMessage(t["Service with same name already exist"]);
  }
}

public class CreateServiceCatalogFromProductAndServiceRequestHandler : IRequestHandler<CreateServiceCatalogFromProductAndServiceRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IApplicationUnitOfWork _uow;
  private IRepositoryWithEvents<Product> _prodRepo;
  private IRepositoryWithEvents<Service> _serviceRepo;
  private readonly ISystemDefaults _systemDefaults;
  private readonly IFileStorageService _file;

  public CreateServiceCatalogFromProductAndServiceRequestHandler(IRepository<ServiceCatalog> repository,
    IApplicationUnitOfWork uow, IRepositoryWithEvents<Product> prodRepo, IRepositoryWithEvents<Service> serviceRepo,
    ISystemDefaults systemDefaults,IFileStorageService file)
  {
    _repository = repository;
    _uow = uow;
    _prodRepo = prodRepo;
    _serviceRepo = serviceRepo;
    _systemDefaults = systemDefaults;
    _file = file;
  }

  public async Task<Guid> Handle(CreateServiceCatalogFromProductAndServiceRequest request, CancellationToken cancellationToken)
  {
    var brand = await _systemDefaults.GetDefaultBrandAsync(cancellationToken);

    string productImagePath = await _file.UploadAsync<Product>(request.Product.Image, FileType.Image, cancellationToken);
    var product = new Product(request.Product.Name, request.Product.Name, request.Price, brand.Id, productImagePath);
    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));
    await _prodRepo.AddAsync(product, cancellationToken);
    //
    // var service = new Service(request.ServiceName, "", null);
    // service.AddDomainEvent(EntityCreatedEvent.WithEntity(service));
    // await _serviceRepo.AddAsync(service, cancellationToken);

    var catalogItem = new ServiceCatalog(request.ServiceId, product.Id, request.Price, request.Priority);
    catalogItem.AddDomainEvent(EntityCreatedEvent.WithEntity(catalogItem));
    await _repository.AddAsync(catalogItem, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return catalogItem.Id;
  }
}