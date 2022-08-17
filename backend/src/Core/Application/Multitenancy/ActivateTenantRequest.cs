using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class ActivateTenantRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;

  public ActivateTenantRequest(string tenantId) => TenantId = tenantId;
}

public class ActivateTenantRequestValidator : CustomValidator<ActivateTenantRequest>
{
  public ActivateTenantRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class ActivateTenantRequestHandler : IRequestHandler<ActivateTenantRequest, string>
{
  private readonly INonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly IStringLocalizer _t;

  public ActivateTenantRequestHandler(IMultiTenantStore<FSHTenantInfo> tenantStore, INonAggregateRepository<FSHTenantInfo> repo,
    IStringLocalizer<ActivateTenantRequestHandler> localizer)
  {
    _tenantStore = tenantStore;
    _repo = repo;
    _t = localizer;
  }

  public async Task<string> Handle(ActivateTenantRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.GetByIdAsync(request.TenantId, cancellationToken);

    if (tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Activated."]);
    }

    tenant.Activate();

    await _tenantStore.TryUpdateAsync(tenant);

    return _t["Tenant {0} is now Activated.", request.TenantId];
  }
}