using FSH.WebApi.Domain.MultiTenancy;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class GetTenantSubscriptionsRequest : IRequest<List<TenantSubscriptionDto>>
{
  public string TenantId { get; set; }
  public bool? ActiveSubscription { get; set; }

  public GetTenantSubscriptionsRequest(string tenantId, bool? activeSubscription)
  {
    TenantId = tenantId;
    ActiveSubscription = activeSubscription;
  }
}

public class GetTenantWithActiveSubscriptions : Specification<FSHTenantInfo>, ISingleResultSpecification
{
  public GetTenantWithActiveSubscriptions(string tenantId, bool? onlyActiveHistory = null) =>
    Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.SubscriptionHistory
        .Where(x => x.TenantId == tenantId && (onlyActiveHistory == null || x.ExpiryDate > DateTime.Now)))
      .Include(a => a.DemoSubscription)
      .Include(a => a.TrainSubscription)
      .Include(a => a.Payments)
      .Where(a => a.Id == tenantId);
}

public class GetTenantSubscriptionsRequestHandler : IRequestHandler<GetTenantSubscriptionsRequest, List<TenantSubscriptionDto>>
{
  private readonly IReadTenantRepository<FSHTenantInfo> _repository;
  private readonly IStringLocalizer _t;

  public GetTenantSubscriptionsRequestHandler(IStringLocalizer<GetTenantSubscriptionsRequestHandler> localizer, IReadTenantRepository<FSHTenantInfo> repository)
  {
    _t = localizer;
    _repository = repository;
  }

  public async Task<List<TenantSubscriptionDto>> Handle(GetTenantSubscriptionsRequest request, CancellationToken cancellationToken)
  {
    List<TenantSubscriptionDto> subscriptions = default!;
    var tenant = await _repository.GetBySpecAsync(new GetTenantWithActiveSubscriptions(request.TenantId, request.ActiveSubscription), cancellationToken);
    if (tenant == null)
    {
      throw new NotFoundException(_t["Tenant {0} is not found", request.TenantId]);
    }

    if (tenant.ProdSubscription != null)
    {
      subscriptions.Add(tenant.ProdSubscription.Adapt<TenantSubscriptionDto>());
    }

    if (tenant.DemoSubscription != null)
    {
      subscriptions.Add(tenant.DemoSubscription.Adapt<TenantSubscriptionDto>());
    }

    if (tenant.TrainSubscription != null)
    {
      subscriptions.Add(tenant.TrainSubscription.Adapt<TenantSubscriptionDto>());
    }

    return subscriptions;
  }
}