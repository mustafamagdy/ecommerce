using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Services;

public class UpdateServiceRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public bool DeleteCurrentImage { get; set; } = false;
  public FileUploadRequest? Image { get; set; }
}

public class UpdateServiceRequestValidator : CustomValidator<UpdateServiceRequest>
{
  public UpdateServiceRequestValidator(IRepository<Service> repository, IStringLocalizer<UpdateServiceRequestValidator> T) =>
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (service, name, ct) =>
        await repository.FirstOrDefaultAsync(new ServiceByNameSpec(name), ct)
          is not Service existingService || existingService.Id == service.Id)
      .WithMessage((_, name) => T["Service {0} already Exists.", name]);
}

public class UpdateServiceRequestHandler : IRequestHandler<UpdateServiceRequest, Guid>
{
  private readonly IRepositoryWithEvents<Service> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateServiceRequestHandler(IRepositoryWithEvents<Service> repository, IStringLocalizer<UpdateServiceRequestHandler> localizer,
    IFileStorageService file, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
    _t = localizer;
  }

  public async Task<Guid> Handle(UpdateServiceRequest request, CancellationToken cancellationToken)
  {
    var service = await _repository.GetByIdAsync(request.Id, cancellationToken);
    _ = service ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);

    if (request.Image != null || request.DeleteCurrentImage)
    {
      service.SetImageUrl(await _file.UploadAsync<Service>(request.Image, FileType.Image, cancellationToken));
      var currentImage = service.ImagePath ?? string.Empty;
      if (request.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
      {
        string root = Directory.GetCurrentDirectory();
        _file.Remove(Path.Combine(root, currentImage));
      }
    }

    string? productImagePath = request.Image is not null
      ? await _file.UploadAsync<Product>(request.Image, FileType.Image, cancellationToken)
      : null;


    var updatedService = service.Update(request.Name, request.Description, productImagePath);

    service.AddDomainEvent(EntityUpdatedEvent.WithEntity(service));

    await _repository.UpdateAsync(updatedService, cancellationToken);

    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}