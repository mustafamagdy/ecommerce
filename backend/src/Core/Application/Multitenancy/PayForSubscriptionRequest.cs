using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class PayForSubscriptionRequest : IRequest
{
  public decimal Amount { get; set; }
  public Guid? PaymentMethodId { get; set; }
  public Guid SubscriptionId { get; set; }
}

public class PayForSubscriptionRequestValidator : CustomValidator<PayForSubscriptionRequest>
{
  public PayForSubscriptionRequestValidator()
  {
    RuleFor(t => t.SubscriptionId).NotNull();
    RuleFor(t => t.Amount).GreaterThan(0).LessThan(MultitenancyConstants.MaxOneTimePaymentAmountForSubscription);
  }
}

public class PayForSubscriptionRequestHandler : IRequestHandler<PayForSubscriptionRequest>
{
  private readonly ITenantService _tenantService;

  public PayForSubscriptionRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

  public Task<Unit> Handle(PayForSubscriptionRequest request, CancellationToken cancellationToken) =>
    _tenantService.PayForSubscription(request.SubscriptionId, request.Amount, request.PaymentMethodId);
}