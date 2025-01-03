using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class UpdateServiceCatalogRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
  public Guid ServiceId { get; set; }
  public Guid ServiceCatalogId { get; set; }
}

public class UpdateServiceCatalogRequestHandler : IRequestHandler<UpdateServiceCatalogRequest, Guid>
{
  private readonly IRepository<ServiceCatalog> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;

  public UpdateServiceCatalogRequestHandler(IRepository<ServiceCatalog> repository, IStringLocalizer<UpdateServiceCatalogRequestHandler> localizer, IFileStorageService file) =>
    (_repository, _t, _file) = (repository, localizer, file);

  public async Task<Guid> Handle(UpdateServiceCatalogRequest request, CancellationToken cancellationToken)
  {
    var serviceCatalog = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = serviceCatalog ?? throw new NotFoundException(_t["ServiceCatalog {0} Not Found.", request.Id]);

    var updatedServiceCatalog = serviceCatalog.Update(request.Price, request.Priority);

    // Add Domain Events to be raised after the commit
    serviceCatalog.DomainEvents.Add(EntityUpdatedEvent.WithEntity(serviceCatalog));

    await _repository.UpdateAsync(updatedServiceCatalog, cancellationToken);

    return request.Id;
  }
}