using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Categories;

public class DeleteCategoryRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteCategoryRequest(Guid id) => Id = id;
}

public class DeleteCategoryRequestHandler : IRequestHandler<DeleteCategoryRequest, Guid>
{
  private readonly IRepository<Category> _repository;
  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;

  public DeleteCategoryRequestHandler(IRepository<Category> repository, IStringLocalizer<DeleteCategoryRequestHandler> localizer,
    IApplicationUnitOfWork uow, IReadRepository<ServiceCatalog> serviceCatalogRepo)
  {
    _repository = repository;
    _uow = uow;
    _serviceCatalogRepo = serviceCatalogRepo;
    _t = localizer;
  }

  public async Task<Guid> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
  {
    if (await _serviceCatalogRepo.AnyAsync(new ServiceCatalogByCategorySpec(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["Category cannot be deleted as it's being used."]);
    }

    var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
    _ = category ?? throw new NotFoundException(_t["Category {0} Not Found.", request.Id]);
    category.AddDomainEvent(EntityDeletedEvent.WithEntity(category));

    await _repository.DeleteAsync(category, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}