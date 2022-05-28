namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest : IRequest<OrderItemDto>
{
  public Guid ItemId { get; set; }
}