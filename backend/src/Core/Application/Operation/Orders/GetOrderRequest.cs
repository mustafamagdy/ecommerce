namespace FSH.WebApi.Application.Operation.Orders;

public class GetOrderRequest
{
  public GetOrderRequest(Guid orderId)
  {
    OrderId = orderId;
  }

  public Guid OrderId { get; private set; }
}