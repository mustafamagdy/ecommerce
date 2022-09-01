using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class DeleteServiceCatalogRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteServiceCatalogRequest(Guid id) => Id = id;
}

public class AnyOrderItemsForServiceCatalogItemId : Specification<OrderItem>, ISingleResultSpecification
{
  public AnyOrderItemsForServiceCatalogItemId(Guid catalogItemId) => Query.Where(a => a.ServiceCatalogId == catalogItemId);
}

public class DeleteServiceCatalogRequestHandler : IRequestHandler<DeleteServiceCatalogRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IReadRepository<OrderItem> _orderItemRepo;

  public DeleteServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository, IStringLocalizer<DeleteServiceCatalogRequestHandler> localizer, IApplicationUnitOfWork uow, IReadRepository<OrderItem> orderItemRepo)
  {
    _repository = repository;
    _uow = uow;
    _orderItemRepo = orderItemRepo;
    _t = localizer;
  }

  public async Task<Guid> Handle(DeleteServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    if (await _orderItemRepo.AnyAsync(new AnyOrderItemsForServiceCatalogItemId(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["Catalog item cannot be deleted as it's being used."]);
    }

    var catalogItem = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = catalogItem ?? throw new NotFoundException(_t["Service Catalog Item {0} Not Found.", request.Id]);

    catalogItem.AddDomainEvent(EntityDeletedEvent.WithEntity(catalogItem));

    await _repository.DeleteAsync(catalogItem, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}