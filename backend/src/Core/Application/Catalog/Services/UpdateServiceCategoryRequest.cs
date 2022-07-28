namespace FSH.WebApi.Application.Catalog.Services;

public class UpdateServiceRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public FileUploadRequest? ImageFile { get; set; }
  public bool DeleteCurrentImage { get; set; }
}

public class UpdateServiceRequestValidator : CustomValidator<UpdateServiceRequest>
{
  public UpdateServiceRequestValidator(IRepository<Service> repository, IStringLocalizer<UpdateServiceRequestValidator> T) =>
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (service, name, ct) =>
        await repository.GetBySpecAsync(new ServiceByNameSpec(name), ct)
          is not Service existingService || existingService.Id == service.Id)
      .WithMessage((_, name) => T["Service {0} already Exists.", name]);
}

public class UpdateServiceRequestHandler : IRequestHandler<UpdateServiceRequest, Guid>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<Service> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _fileStorage;

  public UpdateServiceRequestHandler(IRepositoryWithEvents<Service> repository, IStringLocalizer<UpdateServiceRequestHandler> localizer, IFileStorageService fileStorage)
  {
    _repository = repository;
    _fileStorage = fileStorage;
    _t = localizer;
  }

  public async Task<Guid> Handle(UpdateServiceRequest request, CancellationToken cancellationToken)
  {
    var service = await _repository.GetByIdAsync(request.Id, cancellationToken);
    _ = service ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);

    if (request.ImageFile != null || request.DeleteCurrentImage)
    {
      service.ImageUrl = await _fileStorage.UploadAsync<Service>(request.ImageFile, FileType.Image, cancellationToken);
      var currentImage = service.ImageUrl ?? string.Empty;
      if (request.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
      {
        string root = Directory.GetCurrentDirectory();
        _fileStorage.Remove(Path.Combine(root, currentImage));
      }
    }

    await _repository.UpdateAsync(service, cancellationToken);
    return request.Id;
  }
}