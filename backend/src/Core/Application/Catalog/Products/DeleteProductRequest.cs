using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Products;

public class DeleteProductRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteProductRequest(Guid id) => Id = id;
}

public class DeleteProductRequestHandler : IRequestHandler<DeleteProductRequest, Guid>
{
  private readonly IRepository<Product> _repository;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;

  public DeleteProductRequestHandler(IRepository<Product> repository, IStringLocalizer<DeleteProductRequestHandler> localizer,
    IApplicationUnitOfWork uow, IReadRepository<ServiceCatalog> serviceCatalogRepo)
  {
    _repository = repository;
    _uow = uow;
    _serviceCatalogRepo = serviceCatalogRepo;
    _t = localizer;
  }

  public async Task<Guid> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
  {
    if (await _serviceCatalogRepo.AnyAsync(new ServiceCatalogByProductSpec(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["Product cannot be deleted as it's being used."]);
    }

    var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = product ?? throw new NotFoundException(_t["Product {0} Not Found.", request.Id]);

    product.AddDomainEvent(EntityDeletedEvent.WithEntity(product));

    await _repository.DeleteAsync(product, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}