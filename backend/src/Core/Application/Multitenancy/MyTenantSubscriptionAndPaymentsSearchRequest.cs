using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class MyTenantSubscriptionAndPaymentsSearchRequest : IRequest<ProdTenantSubscriptionWithPaymentDto>
{
  public Guid SubscriptionId { get; set; }
}

public class MyTenantSubscriptionAndPaymentsSearchRequestValidator : CustomValidator<MyTenantSubscriptionAndPaymentsSearchRequest>
{
  public MyTenantSubscriptionAndPaymentsSearchRequestValidator() =>
    RuleFor(t => t.SubscriptionId).NotEmpty();
}

public class MyTenantSubscriptionAndPaymentsSearchRequestHandler : IRequestHandler<MyTenantSubscriptionAndPaymentsSearchRequest, ProdTenantSubscriptionWithPaymentDto>
{
  private readonly FSHTenantInfo _currentTenant;
  private readonly IStringLocalizer _t;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;
  private readonly ISystemTime _systemTime;

  public MyTenantSubscriptionAndPaymentsSearchRequestHandler(FSHTenantInfo currentTenant, IStringLocalizer<MyTenantSubscriptionAndPaymentsSearchRequestHandler> localizer, IReadNonAggregateRepository<FSHTenantInfo> repo, ISystemTime systemTime)
  {
    _currentTenant = currentTenant;
    _t = localizer;
    _repo = repo;
    _systemTime = systemTime;
  }

  public async Task<ProdTenantSubscriptionWithPaymentDto> Handle(MyTenantSubscriptionAndPaymentsSearchRequest request, CancellationToken cancellationToken)
  {
    var tenantId = _currentTenant.Id;
    var tenant = await _repo.FirstOrDefaultAsync(new GetTenantWithActiveSubscriptionsSpec(tenantId, _systemTime), cancellationToken);
    if (tenant == null)
    {
      throw new NotFoundException(_t["Tenant {0} has no subscriptions", tenantId]);
    }

    if (tenant.ProdSubscriptionId != request.SubscriptionId)
    {
      throw new NotFoundException(_t["Subscription {0} not found for the current tenant", request.SubscriptionId]);
    }

    return tenant.ProdSubscription.Adapt<ProdTenantSubscriptionWithPaymentDto>();
  }
}