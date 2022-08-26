using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class CreateServiceCatalogFromProductAndServiceRequest : IRequest<Guid>
{
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
  public string ServiceName { get; set; }
  public string ProductName { get; set; }
}

public class CreateServiceCatalogFromProductAndServiceRequestValidator : CustomValidator<CreateServiceCatalogFromProductAndServiceRequest>
{
  public CreateServiceCatalogFromProductAndServiceRequestValidator(
    IReadRepository<Product> prodRepo,
    IReadRepository<Service> serviceRepo,
    IStringLocalizer<CreateServiceCatalogFromProductAndServiceRequestValidator> t)
  {
    RuleFor(a => a.Price).GreaterThanOrEqualTo(0);
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

public class CreateServiceCatalogFromProductAndServiceRequestHandler : IRequestHandler<CreateServiceCatalogFromProductAndServiceRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IApplicationUnitOfWork _uow;
  private IRepositoryWithEvents<Product> _prodRepo;
  private IRepositoryWithEvents<Service> _serviceRepo;
  private readonly ISystemDefaults _systemDefaults;

  public CreateServiceCatalogFromProductAndServiceRequestHandler(IRepository<ServiceCatalog> repository,
    IApplicationUnitOfWork uow, IRepositoryWithEvents<Product> prodRepo, IRepositoryWithEvents<Service> serviceRepo,
    ISystemDefaults systemDefaults)
  {
    _repository = repository;
    _uow = uow;
    _prodRepo = prodRepo;
    _serviceRepo = serviceRepo;
    _systemDefaults = systemDefaults;
  }

  public async Task<Guid> Handle(CreateServiceCatalogFromProductAndServiceRequest request, CancellationToken cancellationToken)
  {
    var brand = await _systemDefaults.GetDefaultBrandAsync(cancellationToken);

    var product = new Product(request.ProductName, "", request.Price, brand.Id, null);
    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));
    await _prodRepo.AddAsync(product, cancellationToken);

    var service = new Service(request.ServiceName, "", null);
    service.AddDomainEvent(EntityCreatedEvent.WithEntity(service));
    await _serviceRepo.AddAsync(service, cancellationToken);

    var catalogItem = new ServiceCatalog(service.Id, product.Id, request.Price, request.Priority);
    catalogItem.AddDomainEvent(EntityCreatedEvent.WithEntity(catalogItem));
    await _repository.AddAsync(catalogItem, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return catalogItem.Id;
  }
}