namespace FSH.WebApi.Application.Operation.Orders;

public class OrderDto : IDto
{
  public Guid Id { get; set; }
  public string OrderNumber { get; set; }
  public string OrderDate { get; set; }
  public string OrderTime { get; set; }
  public string CustomerName { get; set; }
  public List<OrderItemDto> OrderItems { get; set; }
}

public class OrderItemDto : IDto
{
  public Guid Id { get; set; }
  public string ItemName { get; set; }
  public decimal Price { get; set; }
  public decimal VatPercent { get; set; }
  public decimal VatAmount { get; set; }
  public decimal ItemTotal { get; set; }
}