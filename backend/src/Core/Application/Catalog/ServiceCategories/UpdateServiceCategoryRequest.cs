namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class UpdateServiceCategoryRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public string? IconUrl { get; set; }
}

public class UpdateServiceCategoryRequestValidator : CustomValidator<UpdateServiceCategoryRequest>
{
  public UpdateServiceCategoryRequestValidator(IRepository<ServiceCategory> repository,
    IStringLocalizer<UpdateServiceCategoryRequestValidator>
      T) =>
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (serviceCategory, name, ct) =>
        await repository.GetBySpecAsync(new ServiceCategoryByNameSpec(name), ct)
          is not ServiceCategory existingServiceCategory || existingServiceCategory.Id == serviceCategory.Id)
      .WithMessage((_, name) => T["ServiceCategory {0} already Exists.", name]);
}

public class UpdateServiceCategoryRequestHandler : IRequestHandler<UpdateServiceCategoryRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<ServiceCategory> _repository;
  private readonly IStringLocalizer _t;

  public UpdateServiceCategoryRequestHandler(IRepositoryWithEvents<ServiceCategory> repository, IStringLocalizer<UpdateServiceCategoryRequestHandler> localizer) =>
    (_repository, _t) = (repository, localizer);

  public async Task<Guid> Handle(UpdateServiceCategoryRequest request, CancellationToken cancellationToken)
  {
    var serviceCategory = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = serviceCategory
        ?? throw new NotFoundException(_t["ServiceCategory {0} Not Found.", request.Id]);

    serviceCategory.Update(request.Name, request.Description, request.ImageUrl, request.IconUrl);

    await _repository.UpdateAsync(serviceCategory, cancellationToken);

    return request.Id;
  }
}