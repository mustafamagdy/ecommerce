using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public class SubscriptionPayment : Payment
{
  public SubscriptionPayment(decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
  }
}