namespace FSH.WebApi.Application.Catalog.Services;

public class CreateServiceRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public FileUploadRequest ImageFile { get; set; }
}

public class CreateServiceRequestValidator : CustomValidator<CreateServiceRequest>
{
  public CreateServiceRequestValidator(IReadRepository<Service> repository, IStringLocalizer<CreateServiceRequestValidator> T)
    => RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new ServiceByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["Service {0} already Exists.", name]);
}

public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<Service> _repository;
  private readonly IFileStorageService _fileStorage;

  public CreateServiceRequestHandler(IRepositoryWithEvents<Service> repository, IFileStorageService fileStorage)
  {
    _repository = repository;
    _fileStorage = fileStorage;
  }

  public async Task<Guid> Handle(CreateServiceRequest request, CancellationToken cancellationToken)
  {
    var imageUrl = await _fileStorage.UploadAsync<Service>(request.ImageFile, FileType.Image, cancellationToken);
    var service = new Service(request.Name, request.Description, imageUrl);

    await _repository.AddAsync(service, cancellationToken);

    return service.Id;
  }
}