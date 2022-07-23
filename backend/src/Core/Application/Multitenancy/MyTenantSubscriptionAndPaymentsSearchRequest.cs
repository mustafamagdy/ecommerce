using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class MyTenantSubscriptionAndPaymentsSearchRequest : IRequest<ProdTenantSubscriptionDto>
{
  public Guid SubscriptionId { get; set; }
}

public class MyTenantSubscriptionAndPaymentsSearchRequestValidator : CustomValidator<MyTenantSubscriptionAndPaymentsSearchRequest>
{
  public MyTenantSubscriptionAndPaymentsSearchRequestValidator() =>
    RuleFor(t => t.SubscriptionId).NotEmpty();
}

public class MyTenantSubscriptionAndPaymentsSearchRequestHandler : IRequestHandler<MyTenantSubscriptionAndPaymentsSearchRequest, ProdTenantSubscriptionDto>
{
  private readonly FSHTenantInfo _currentTenant;
  private readonly IStringLocalizer _t;
  private readonly IReadTenantRepository<FSHTenantInfo> _repo;
  private readonly ISystemTime _systemTime;

  public MyTenantSubscriptionAndPaymentsSearchRequestHandler(FSHTenantInfo currentTenant, IStringLocalizer<MyTenantSubscriptionAndPaymentsSearchRequestHandler> localizer, IReadTenantRepository<FSHTenantInfo> repo, ISystemTime systemTime)
  {
    _currentTenant = currentTenant;
    _t = localizer;
    _repo = repo;
    _systemTime = systemTime;
  }

  public async Task<ProdTenantSubscriptionDto> Handle(MyTenantSubscriptionAndPaymentsSearchRequest request, CancellationToken cancellationToken)
  {
    var tenantId = _currentTenant.Id;
    var tenant = await _repo.GetBySpecAsync(new GetTenantWithActiveSubscriptionsSpec(tenantId, _systemTime), cancellationToken);
    if (tenant == null)
    {
      throw new NotFoundException(_t["Tenant {0} has no subscriptions", tenantId]);
    }

    if (tenant.ProdSubscriptionId != request.SubscriptionId)
    {
      throw new NotFoundException(_t["Subscription {0} not found for the current tenant", request.SubscriptionId]);
    }

    return tenant.ProdSubscription.Adapt<ProdTenantSubscriptionDto>();
  }
}