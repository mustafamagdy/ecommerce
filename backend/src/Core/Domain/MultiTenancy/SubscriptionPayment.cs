using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public class SubscriptionPayment : Payment
{
  private SubscriptionPayment()
  {
  }

  public SubscriptionPayment(TenantProdSubscription tenantProdSubscription, decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
    TenantProdSubscription = tenantProdSubscription;
    TenantProdSubscriptionId = tenantProdSubscription.Id;
  }

  public Guid TenantProdSubscriptionId { get; set; }
  public TenantProdSubscription TenantProdSubscription { get; set; }
}