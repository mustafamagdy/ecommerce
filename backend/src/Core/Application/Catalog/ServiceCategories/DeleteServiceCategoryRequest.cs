using FSH.WebApi.Application.Catalog.Services;

namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class DeleteServiceCategoryRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteServiceCategoryRequest(Guid id) => Id = id;
}

public class DeleteServiceCategoryRequestHandler : IRequestHandler<DeleteServiceCategoryRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<ServiceCategory> _serviceCategoryRepo;
  private readonly IReadRepository<Service> _serviceRepo;
  private readonly IStringLocalizer _t;

  public DeleteServiceCategoryRequestHandler(IRepositoryWithEvents<ServiceCategory> serviceCategoryRepo,
    IReadRepository<Service>
      serviceRepo,
    IStringLocalizer<DeleteServiceCategoryRequestHandler> localizer) =>
    (_serviceCategoryRepo, _serviceRepo, _t) = (serviceCategoryRepo, serviceRepo, localizer);

  public async Task<Guid> Handle(DeleteServiceCategoryRequest request, CancellationToken cancellationToken)
  {
    if (await _serviceRepo.AnyAsync(new ServicesByServiceCategorySpec(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["ServiceCategory cannot be deleted as it's being used."]);
    }

    var serviceCategory = await _serviceCategoryRepo.GetByIdAsync(request.Id, cancellationToken);

    _ = serviceCategory ?? throw new NotFoundException(_t["ServiceCategory {0} Not Found."]);

    await _serviceCategoryRepo.DeleteAsync(serviceCategory, cancellationToken);

    return request.Id;
  }
}