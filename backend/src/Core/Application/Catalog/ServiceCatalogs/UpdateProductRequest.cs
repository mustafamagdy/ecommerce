using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class UpdateServiceCatalogRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
  public Guid? ServiceId { get; set; }
  public Guid? ProductId { get; set; }
}

public class UpdateServiceCatalogRequestHandler : IRequestHandler<UpdateServiceCatalogRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uwo;

  public UpdateServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository, IStringLocalizer<UpdateServiceCatalogRequestHandler> localizer, IFileStorageService file, IApplicationUnitOfWork uwo)
  {
    _repository = repository;
    _file = file;
    _uwo = uwo;
    _t = localizer;
  }

  public async Task<Guid> Handle(UpdateServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    var serviceCatalog = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = serviceCatalog ?? throw new NotFoundException(_t["ServiceCatalog {0} Not Found.", request.Id]);

    var updatedServiceCatalog = serviceCatalog.Update(request.ProductId, request.ServiceId, request.Price, request.Priority);

    serviceCatalog.AddDomainEvent(EntityUpdatedEvent.WithEntity(serviceCatalog));

    await _repository.UpdateAsync(updatedServiceCatalog, cancellationToken);
    await _uwo.CommitAsync(cancellationToken);
    return request.Id;
  }
}