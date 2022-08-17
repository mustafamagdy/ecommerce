using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class DeactivateTenantRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;

  public DeactivateTenantRequest(string tenantId) => TenantId = tenantId;
}

public class DeactivateTenantRequestValidator : CustomValidator<DeactivateTenantRequest>
{
  public DeactivateTenantRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class DeactivateTenantRequestHandler : IRequestHandler<DeactivateTenantRequest, string>
{
  private readonly INonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly IStringLocalizer _t;

  public DeactivateTenantRequestHandler(IMultiTenantStore<FSHTenantInfo> tenantStore, INonAggregateRepository<FSHTenantInfo> repo,
    IStringLocalizer<ActivateTenantRequestHandler> localizer)
  {
    _tenantStore = tenantStore;
    _repo = repo;
    _t = localizer;
  }

  public async Task<string> Handle(DeactivateTenantRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.GetByIdAsync(request.TenantId, cancellationToken);

    if (!tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Deactivated."]);
    }

    tenant.DeActivate();

    await _tenantStore.TryUpdateAsync(tenant);

    return _t[$"Tenant {0} is now Deactivated.", request.TenantId];
  }
}