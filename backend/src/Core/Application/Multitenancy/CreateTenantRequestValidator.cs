using FSH.WebApi.Application.Multitenancy.Services;

namespace FSH.WebApi.Application.Multitenancy;

public class CreateTenantRequestValidator : CustomValidator<CreateTenantRequest>
{
  public CreateTenantRequestValidator(
    ITenantService tenantService,
    IStringLocalizer<CreateTenantRequestValidator> T,
    IConnectionStringValidator connectionStringValidator)
  {
    RuleFor(t => t.Id)
      .NotEmpty()
      .MustAsync(async (id, _) => !await tenantService.ExistsWithIdAsync(id))
      .WithMessage((_, id) => T["Tenant {0} already exists.", id]);

    RuleFor(t => t.Name)
      .NotEmpty()
      .MustAsync(async (name, _) => !await tenantService.ExistsWithNameAsync(name!))
      .WithMessage((_, name) => T["Tenant {0} already exists.", name]);

    RuleFor(t => t.AdminEmail)
      .NotEmpty()
      .EmailAddress();

    RuleFor(t => t.TechSupportUserId)
      .NotEmpty();

    RuleFor(a => a.ProdPackageId).NotEmpty().When(a => a.DemoPackageId == null);
    RuleFor(a => a.DemoPackageId).NotEmpty().When(a => a.ProdPackageId == null);
  }
}