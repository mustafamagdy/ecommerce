using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Services;

public class CreateServiceRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public decimal Rate { get; set; }
  public Guid ServiceCategoryId { get; set; }
  public FileUploadRequest? Image { get; set; }
}

public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequest, Guid>
{
  private readonly IRepository<Service> _repository;
  private readonly IFileStorageService _file;

  public CreateServiceRequestHandler(IRepository<Service> repository, IFileStorageService file) =>
    (_repository, _file) = (repository, file);

  public async Task<Guid> Handle(CreateServiceRequest request, CancellationToken cancellationToken)
  {
    string serviceImagePath = await _file.UploadAsync<Service>(request.Image, FileType.Image, cancellationToken);
    string serviceIconPath = await _file.UploadAsync<Service>(request.Image, FileType.Image, cancellationToken);

    var service = new Service(request.Name, request.Description, request.ServiceCategoryId, serviceImagePath, serviceIconPath);

    // Add Domain Events to be raised after the commit
    service.DomainEvents.Add(EntityCreatedEvent.WithEntity(service));

    await _repository.AddAsync(service, cancellationToken);

    return service.Id;
  }
}