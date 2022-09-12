namespace FSH.WebApi.Domain.Operation;

public sealed class OrderPayment : Payment
{
  private OrderPayment()
  {
  }

  public Guid OrderId { get; private set; }
  public Order Order { get; set; }

  public OrderPayment(Guid orderId, Guid paymentMethodId, decimal amount)
    : base(amount, paymentMethodId)
  {
    OrderId = orderId;
  }
}

public abstract class Payment : AuditableEntity, IAggregateRoot
{
  protected Payment()
  {
  }

  protected Payment(decimal amount, Guid paymentMethodId)
  {
    Amount = amount;
    PaymentMethodId = paymentMethodId;
  }

  public decimal Amount { get; protected set; }
  public Guid PaymentMethodId { get; protected set; }
  public PaymentMethod PaymentMethod { get; set; }
}