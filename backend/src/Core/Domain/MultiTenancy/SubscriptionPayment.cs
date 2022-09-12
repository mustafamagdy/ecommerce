using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public sealed class SubscriptionPayment : Payment
{
  private SubscriptionPayment()
  {
  }

  public SubscriptionPayment(decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
  }

  public Guid TenantProdSubscriptionId { get; set; }
  public TenantProdSubscription TenantProdSubscription { get; set; }
}