using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class RenewSubscriptionRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;
  public SubscriptionType SubscriptionType { get; set; }
}

public class RenewSubscriptionRequestValidator : CustomValidator<RenewSubscriptionRequest>
{
  public RenewSubscriptionRequestValidator()
  {
    RuleFor(t => t.TenantId).NotEmpty();
    RuleFor(t => t.SubscriptionType).NotNull();
  }
}

public class RenewSubscriptionRequestHandler : IRequestHandler<RenewSubscriptionRequest, string>
{
  private readonly ITenantService _tenantService;
  private readonly ISystemTime _systemTime;
  private readonly ITenantUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public RenewSubscriptionRequestHandler(ITenantService tenantService, ISystemTime systemTime, ITenantUnitOfWork uow,
    IStringLocalizer<RenewSubscriptionRequestHandler> localizer)
  {
    _tenantService = tenantService;
    _systemTime = systemTime;
    _uow = uow;
    _t = localizer;
  }

  public async Task<string> Handle(RenewSubscriptionRequest request, CancellationToken cancellationToken)
  {
    var subscription = await _tenantService.GetSubscription<Subscription>(request.SubscriptionType);
    var tenant = await _tenantService.GetTenantById(request.TenantId);
    var newExpiryDate = subscription switch
    {
      StandardSubscription => tenant.ProdSubscription?.Renew(_systemTime.Now).ExpiryDate,
      DemoSubscription => tenant.DemoSubscription?.Renew(_systemTime.Now).ExpiryDate,
      TrainSubscription => tenant.TrainSubscription?.Renew(_systemTime.Now).ExpiryDate,
      _ => null
    };

    await _uow.CommitAsync(cancellationToken);

    return _t["Subscription {0} renewed. Now Valid till {1}.", subscription.GetType().Name, newExpiryDate];
  }
}