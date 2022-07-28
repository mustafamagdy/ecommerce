using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

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

  public RenewSubscriptionRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

  public async Task<string> Handle(RenewSubscriptionRequest request, CancellationToken cancellationToken)
  {
    var subscription = await _tenantService.GetSubscription<Subscription>(request.SubscriptionType);
    var tenant = await _tenantService.GetTenantById(request.TenantId);
    return await _tenantService.RenewSubscription(tenant, subscription);
  }
}