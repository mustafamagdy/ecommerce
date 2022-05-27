namespace FSH.WebApi.Domain.Operation;

public class Order : AuditableEntity, IAggregateRoot
{
  public Guid CustomerId { get; private set; }
  public virtual Customer Customer { get; private set; } = default!;

  public Order(Guid customerId)
  {
    CustomerId = customerId;
  }

  public Order Update(Guid? customerId)
  {
    if (customerId is not null && !CustomerId.Equals(customerId.Value)) CustomerId = customerId.Value;
    return this;
  }
}