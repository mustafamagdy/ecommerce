namespace FSH.WebApi.Application.Catalog.Services;

public class UpdateServiceRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public string? IconUrl { get; set; }
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

  public UpdateServiceRequestHandler(IRepositoryWithEvents<Service> repository, IStringLocalizer<UpdateServiceRequestHandler> localizer) =>
    (_repository, _t) = (repository, localizer);

  public async Task<Guid> Handle(UpdateServiceRequest request, CancellationToken cancellationToken)
  {
    var service = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = service ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);

    service.Update(request.Name, request.Description, request.ImageUrl);

    await _repository.UpdateAsync(service, cancellationToken);

    return request.Id;
  }
}