using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;

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
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
  private readonly IReadNonAggregateRepository<TenantSubscription> _tenantSubscriptionRepo;
  private readonly ITenantUnitOfWork _uow;

  public PayForSubscriptionRequestHandler(IReadRepository<PaymentMethod> paymentMethodRepo,
    IReadNonAggregateRepository<TenantSubscription> tenantSubscriptionRepo, ITenantUnitOfWork uow,
    IReadNonAggregateRepository<FSHTenantInfo> tenantRepo)
  {
    _paymentMethodRepo = paymentMethodRepo;
    _tenantSubscriptionRepo = tenantSubscriptionRepo;
    _uow = uow;
    _tenantRepo = tenantRepo;
  }

  public async Task<Unit> Handle(PayForSubscriptionRequest request, CancellationToken cancellationToken)
  {
    PaymentMethod pm;
    if (request.PaymentMethodId == null)
    {
      pm = await _paymentMethodRepo.FirstOrDefaultAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken)
           ?? throw new ArgumentOutOfRangeException(nameof(request.PaymentMethodId), "No default cash payment method register");
    }
    else
    {
      pm = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId.Value, cancellationToken)
           ?? throw new NotFoundException($"Payment method {request.PaymentMethodId} not found");
    }

    var subscription = await _tenantSubscriptionRepo.GetByIdAsync(request.SubscriptionId, cancellationToken) as TenantProdSubscription;
    var tenant = await _tenantRepo.GetByIdAsync(subscription.TenantId, cancellationToken);

    subscription.AddDomainEvent(new TenantPayForSubscriptionEvent(tenant, subscription, pm, request.Amount));
    subscription.Pay(request.Amount, pm.Id);

    await _uow.CommitAsync(cancellationToken);

    return Unit.Value;
  }
}