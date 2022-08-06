using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class DeleteServiceCatalogRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteServiceCatalogRequest(Guid id) => Id = id;
}

public class DeleteServiceCatalogRequestHandler : IRequestHandler<DeleteServiceCatalogRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;

  public DeleteServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository, IStringLocalizer<DeleteServiceCatalogRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
    _t = localizer;
  }

  public async Task<Guid> Handle(DeleteServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    var catalogItem = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = catalogItem ?? throw new NotFoundException(_t["Service Catalog Item {0} Not Found.", request.Id]);

    // Add Domain Events to be raised after the commit
    catalogItem.AddDomainEvent(EntityDeletedEvent.WithEntity(catalogItem));

    await _repository.DeleteAsync(catalogItem, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}