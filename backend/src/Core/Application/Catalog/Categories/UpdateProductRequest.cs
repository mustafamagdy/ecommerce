using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Categories;

public class UpdateCategoryRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public class UpdateCategoryRequestValidator : CustomValidator<UpdateCategoryRequest>
{
  public UpdateCategoryRequestValidator(
    IReadRepository<Category> categoryRepo,
    IStringLocalizer<UpdateCategoryRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (product, name, ct) =>
        await categoryRepo.FirstOrDefaultAsync(new CategoryByNameSpec(name), ct)
          is not { } existingCategory || existingCategory.Id == product.Id)
      .WithMessage((_, name) => T["Category {0} already Exists.", name]);
  }
}

public class UpdateCategoryRequestHandler : IRequestHandler<UpdateCategoryRequest, Guid>
{
  private readonly IRepository<Category> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateCategoryRequestHandler(IRepository<Category> repository,
    IStringLocalizer<UpdateCategoryRequestHandler> localizer, IFileStorageService file, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
    _t = localizer;
  }

  public async Task<Guid> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
  {
    var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = product ?? throw new NotFoundException(_t["Category {0} Not Found.", request.Id]);
    var updatedCategory = product.Update(request.Name, request.Description);

    product.AddDomainEvent(EntityUpdatedEvent.WithEntity(product));

    await _repository.UpdateAsync(updatedCategory, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}