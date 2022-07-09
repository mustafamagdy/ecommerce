namespace FSH.WebApi.Application.Multitenancy;

public class CreateTenantRequestValidator : CustomValidator<CreateTenantRequest>
{
  public CreateTenantRequestValidator(
    ITenantService tenantService,
    IStringLocalizer<CreateTenantRequestValidator> T,
    IConnectionStringValidator connectionStringValidator)
  {
    RuleFor(t => t.Id).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MustAsync(async (id, _) => !await tenantService.ExistsWithIdAsync(id))
      .WithMessage((_, id) => T["Tenant {0} already exists.", id]);

    RuleFor(t => t.Name).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MustAsync(async (name, _) => !await tenantService.ExistsWithNameAsync(name!))
      .WithMessage((_, name) => T["Tenant {0} already exists.", name]);

    RuleFor(t => t.DatabaseName).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MustAsync(async (dbName, _) => !await tenantService.DatabaseExistAsync(dbName!))
      .WithMessage((_, dbName) => T["Database {0} already exists.", dbName!]);

    RuleFor(t => t.AdminEmail).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .EmailAddress();
  }
}