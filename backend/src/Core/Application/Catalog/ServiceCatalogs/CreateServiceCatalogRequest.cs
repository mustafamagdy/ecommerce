using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class CreateServiceCatalogRequest : IRequest<Guid>
{
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
  public Guid ServiceId { get; set; }
  public Guid ProductId { get; set; }
}

public class CreateServiceCatalogRequestHandler : IRequestHandler<CreateServiceCatalogRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;

  public CreateServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository) =>
    _repository = repository;

  public async Task<Guid> Handle(CreateServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    var product = new ServiceCatalog(request.ServiceId, request.ProductId, request.Price, request.Priority);

    // Add Domain Events to be raised after the commit
    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));

    await _repository.AddAsync(product, cancellationToken);

    return product.Id;
  }
}