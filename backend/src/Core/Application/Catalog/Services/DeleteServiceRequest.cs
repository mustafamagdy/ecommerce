using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Services;

public class DeleteServiceRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteServiceRequest(Guid id) => Id = id;
}

public class DeleteServiceRequestHandler : IRequestHandler<DeleteServiceRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepositoryWithEvents<Service> _serviceRepo;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;

  public DeleteServiceRequestHandler(IReadRepository<ServiceCatalog> serviceCatalogRepo, IRepositoryWithEvents<Service> serviceRepo,
    IStringLocalizer<DeleteServiceRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _serviceCatalogRepo = serviceCatalogRepo;
    _serviceRepo = serviceRepo;
    _uow = uow;
    _t = localizer;
  }

  public async Task<Guid> Handle(DeleteServiceRequest request, CancellationToken cancellationToken)
  {
    if (await _serviceCatalogRepo.AnyAsync(new ServiceCatalogByServiceSpec(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["Service cannot be deleted as it's being used in service catalogs."]);
    }

    var service = await _serviceRepo.GetByIdAsync(request.Id, cancellationToken);

    _ = service ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);

    await _serviceRepo.DeleteAsync(service, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}