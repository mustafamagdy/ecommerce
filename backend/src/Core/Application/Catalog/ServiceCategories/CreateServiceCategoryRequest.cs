namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class CreateServiceCategoryRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public string? IconUrl { get; set; }
}

public class CreateServiceCategoryRequestValidator : CustomValidator<CreateServiceCategoryRequest>
{
  public CreateServiceCategoryRequestValidator(IReadRepository<ServiceCategory> repository,
    IStringLocalizer<CreateServiceCategoryRequestValidator>
      T) =>
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new ServiceCategoryByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["ServiceCategory {0} already Exists.", name]);
}

public class CreateServiceCategoryRequestHandler : IRequestHandler<CreateServiceCategoryRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<ServiceCategory> _repository;

  public CreateServiceCategoryRequestHandler(IRepositoryWithEvents<ServiceCategory> repository) => _repository =
    repository;

  public async Task<Guid> Handle(CreateServiceCategoryRequest request, CancellationToken cancellationToken)
  {
    var serviceCategory = new ServiceCategory(request.Name, request.Description, request.ImageUrl, request.IconUrl);

    await _repository.AddAsync(serviceCategory, cancellationToken);

    return serviceCategory.Id;
  }
}