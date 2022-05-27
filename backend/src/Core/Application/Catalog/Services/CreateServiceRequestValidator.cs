namespace FSH.WebApi.Application.Catalog.Services;

public class CreateServiceRequestValidator : CustomValidator<CreateServiceRequest>
{
  public CreateServiceRequestValidator(IReadRepository<Service> productRepo, IReadRepository<ServiceCategory>
    brandRepo, IStringLocalizer<CreateServiceRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await productRepo.GetBySpecAsync(new ServiceByNameSpec(name), ct) is null)
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