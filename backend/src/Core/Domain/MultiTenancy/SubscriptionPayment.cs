using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public class SubscriptionPayment : Payment
{
  public SubscriptionPayment()
  {
  }

  public SubscriptionPayment(decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
  }

  public SubscriptionPayment SetAmount(decimal amount)
  {
    Amount = amount;
    return this;
  }

  public SubscriptionPayment SetPaymentMethodId(Guid paymentMethodId)
  {
    PaymentMethodId = paymentMethodId;
    return this;
  }
}