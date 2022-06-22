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

public class GetTenantSubscriptionsRequestHandler : IRequestHandler<GetTenantSubscriptionsRequest, List<TenantSubscriptionDto>>
{
  private readonly ITenantService _tenantService;
  private readonly IStringLocalizer _t;

  public GetTenantSubscriptionsRequestHandler(ITenantService tenantService, IStringLocalizer<GetTenantSubscriptionsRequestHandler> localizer)
  {
    _tenantService = tenantService;
    _t = localizer;
  }

  public async Task<List<TenantSubscriptionDto>> Handle(GetTenantSubscriptionsRequest request, CancellationToken
    cancellationToken)
  {
    List<TenantSubscription> subscriptions = default!;
    if (request.ActiveSubscription == true)
    {
      subscriptions = (await _tenantService.GetActiveSubscriptions(request.TenantId)).ToList();
    }
    else if (request.ActiveSubscription == null)
    {
      subscriptions = await _tenantService.GetAllTenantSubscriptions(request.TenantId);
    }

    if (subscriptions?.Count == 0)
    {
      throw new NotFoundException(_t["Tenant has no active subscriptions"]);
    }

    return subscriptions.Adapt<List<TenantSubscriptionDto>>();
  }
}