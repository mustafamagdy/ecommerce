namespace FSH.WebApi.Application.Operation.Orders;

public class BaseOrderRequest
{
  public List<OrderItemRequest> Items { get; set; }
}