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

  public Task<string> Handle(RenewSubscriptionRequest request, CancellationToken cancellationToken) =>
    _tenantService.RenewSubscription(request.SubscriptionType, request.TenantId);
}