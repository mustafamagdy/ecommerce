namespace FSH.WebApi.Application.Operation.Orders;

public class OrderCanceledEvent : DomainEvent
{
  public OrderCanceledEvent(Guid orderId, DateTime cancellationDate)
  {
    OrderId = orderId;
    CancellationDate = cancellationDate;
  }

  public Guid OrderId { get; }
  public DateTime CancellationDate { get; }
}