using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Services;

public class UpdateServiceRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public decimal Rate { get; set; }
  public Guid ServiceCategoryId { get; set; }
  public bool DeleteCurrentImage { get; set; } = false;
  public FileUploadRequest? Image { get; set; }
}

public class UpdateServiceRequestHandler : IRequestHandler<UpdateServiceRequest, Guid>
{
  private readonly IRepository<Service> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;

  public UpdateServiceRequestHandler(IRepository<Service> repository, IStringLocalizer<UpdateServiceRequestHandler>
    localizer, IFileStorageService file) =>
    (_repository, _t, _file) = (repository, localizer, file);

  public async Task<Guid> Handle(UpdateServiceRequest request, CancellationToken cancellationToken)
  {
    var service = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = service ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);

    // Remove old image if flag is set
    if (request.DeleteCurrentImage)
    {
      string? currentServiceImagePath = service.ImagePath;
      if (!string.IsNullOrEmpty(currentServiceImagePath))
      {
        string root = Directory.GetCurrentDirectory();
        _file.Remove(Path.Combine(root, currentServiceImagePath));
      }

      service = service.ClearImagePath();
    }

    string? serviceImagePath = request.Image is not null
      ? await _file.UploadAsync<Service>(request.Image, FileType.Image, cancellationToken)
      : null;

    string? serviceIconPath = request.Image is not null
      ? await _file.UploadAsync<Service>(request.Image, FileType.Image, cancellationToken)
      : null;

    var updatedService = service.Update(request.Name, request.Description, request.ServiceCategoryId, serviceImagePath, serviceIconPath);

    // Add Domain Events to be raised after the commit
    service.DomainEvents.Add(EntityUpdatedEvent.WithEntity(service));

    await _repository.UpdateAsync(updatedService, cancellationToken);

    return request.Id;
  }
}