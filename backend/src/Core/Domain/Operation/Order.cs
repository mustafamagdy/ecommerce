namespace FSH.WebApi.Domain.Operation;

public class Order : AuditableEntity, IAggregateRoot
{
  public string OrderNumber { get; private set; }
  public Guid CustomerId { get; private set; }
  public virtual Customer Customer { get; private set; } = default!;
  public virtual HashSet<OrderItem> OrderItems { get; set; }

  public Order(Guid customerId, string orderNumber)
  {
    CustomerId = customerId;
    OrderNumber = orderNumber;
  }

  public Order Update(Guid? customerId)
  {
    if (customerId is not null && !CustomerId.Equals(customerId.Value)) CustomerId = customerId.Value;
    return this;
  }
}