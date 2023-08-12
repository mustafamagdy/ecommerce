using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

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
  private readonly IApplicationUnitOfWork _uow;

  public CreateServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    var product = new ServiceCatalog(request.ServiceId, request.ProductId, request.Price, request.Priority);

    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));

    await _repository.AddAsync(product, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return product.Id;
  }
}