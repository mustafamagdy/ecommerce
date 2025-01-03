namespace FSH.WebApi.Domain.Operation;

public class OrderPayment : Payment
{
  public Guid OrderId { get; private set; }
  public virtual Order Order { get; set; }

  public OrderPayment(Guid orderId, Guid paymentMethodId, decimal amount)
    : base(amount, paymentMethodId)
  {
    OrderId = orderId;
  }
}

public abstract class Payment : AuditableEntity, IAggregateRoot
{
  public Payment()
  {
  }

  protected Payment(decimal amount, Guid paymentMethodId)
  {
    Amount = amount;
    PaymentMethodId = paymentMethodId;
  }

  public decimal Amount { get; protected set; }
  public Guid PaymentMethodId { get; protected set; }
  public virtual PaymentMethod PaymentMethod { get; set; } = default!;
}