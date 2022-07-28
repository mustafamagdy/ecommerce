using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class MyTenantSubscriptionSearchRequest : IRequest<TenantSubscriptionDto>
{
  public Guid SubscriptionId { get; set; }
}

public class MyTenantSubscriptionSearchRequestValidator : CustomValidator<MyTenantSubscriptionSearchRequest>
{
  public MyTenantSubscriptionSearchRequestValidator() =>
    RuleFor(t => t.SubscriptionId).NotEmpty();
}

public class MyTenantSubscriptionSearchRequestHandler : IRequestHandler<MyTenantSubscriptionSearchRequest, TenantSubscriptionDto>
{
  private readonly FSHTenantInfo _currentTenant;
  private readonly IStringLocalizer _t;
  private readonly IReadTenantRepository<FSHTenantInfo> _repo;
  private readonly ISystemTime _systemTime;
  private readonly IReadTenantRepository<SubscriptionHistory> _subscriptionHistoryRepo;

  public MyTenantSubscriptionSearchRequestHandler(FSHTenantInfo currentTenant, IStringLocalizer<MyTenantSubscriptionSearchRequestHandler> localizer, IReadTenantRepository<FSHTenantInfo> repo, ISystemTime systemTime, IReadTenantRepository<SubscriptionHistory> subscriptionHistoryRepo)
  {
    _currentTenant = currentTenant;
    _t = localizer;
    _repo = repo;
    _systemTime = systemTime;
    _subscriptionHistoryRepo = subscriptionHistoryRepo;
  }

  public async Task<TenantSubscriptionDto> Handle(MyTenantSubscriptionSearchRequest request, CancellationToken cancellationToken)
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