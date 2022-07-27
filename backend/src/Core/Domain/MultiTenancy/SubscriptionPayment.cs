using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public class SubscriptionPayment : Payment
{
  public SubscriptionPayment()
  {
  }

  public SubscriptionPayment(Guid tenantProdSubscriptionId, decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
    TenantProdSubscriptionId = tenantProdSubscriptionId;
  }

  public Guid TenantProdSubscriptionId { get; set; }
  public virtual TenantProdSubscription TenantProdSubscription { get; set; }
}