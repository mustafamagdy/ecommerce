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

public class GetTenantWithActiveSubscriptionsSpec : Specification<FSHTenantInfo>, ISingleResultSpecification
{
  public GetTenantWithActiveSubscriptionsSpec(string tenantId, ISystemTime systemTime, bool? onlyActiveHistory = null) =>
    Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History.Where(x => onlyActiveHistory == null || x.ExpiryDate >= systemTime.Now))
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.Payments)
      .Include(a => a.DemoSubscription)
      .Include(a => a.TrainSubscription)
      .Where(a => a.Id == tenantId);
}

public class GetTenantSubscriptionsRequestHandler : IRequestHandler<GetTenantSubscriptionsRequest, List<TenantSubscriptionDto>>
{
  private readonly IReadTenantRepository<FSHTenantInfo> _repository;
  private readonly IStringLocalizer _t;
  private readonly ISystemTime _systemTime;

  public GetTenantSubscriptionsRequestHandler(IStringLocalizer<GetTenantSubscriptionsRequestHandler> localizer, IReadTenantRepository<FSHTenantInfo> repository, ISystemTime systemTime)
  {
    _t = localizer;
    _repository = repository;
    _systemTime = systemTime;
  }

  public async Task<List<TenantSubscriptionDto>> Handle(GetTenantSubscriptionsRequest request, CancellationToken cancellationToken)
  {
    List<TenantSubscriptionDto> subscriptions = default!;
    var tenant = await _repository.FirstOrDefaultAsync(new GetTenantWithActiveSubscriptionsSpec(request.TenantId, _systemTime, request.ActiveSubscription), cancellationToken);
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