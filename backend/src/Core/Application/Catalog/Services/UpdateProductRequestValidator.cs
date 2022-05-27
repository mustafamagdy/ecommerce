namespace FSH.WebApi.Application.Catalog.Services;

public class UpdateServiceRequestValidator : CustomValidator<UpdateServiceRequest>
{
  public UpdateServiceRequestValidator(IReadRepository<Service> serviceRepo, IReadRepository<ServiceCategory> brandRepo,
    IStringLocalizer<UpdateServiceRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (service, name, ct) =>
        await serviceRepo.GetBySpecAsync(new ServiceByNameSpec(name), ct)
          is not { } existingService || existingService.Id == service.Id)
      .WithMessage((_, name) => T["Service {0} already Exists.", name]);

    RuleFor(p => p.Rate)
      .GreaterThanOrEqualTo(1);

    RuleFor(p => p.Image)
      .InjectValidator();

    RuleFor(p => p.ServiceCategoryId)
      .NotEmpty()
      .MustAsync(async (id, ct) => await brandRepo.GetByIdAsync(id, ct) is not null)
      .WithMessage((_, id) => T["ServiceCategory {0} Not Found.", id]);
  }
}