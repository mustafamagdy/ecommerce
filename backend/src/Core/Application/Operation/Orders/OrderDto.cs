namespace FSH.WebApi.Application.Operation.Orders;

public class OrderDto : IDto
{
  public Guid Id { get; set; }
  public string OrderNumber { get; set; }
  public string OrderDate { get; set; }
  public string OrderTime { get; set; }
  public string PhoneNumber { get; set; }
  public string CustomerName { get; set; }
  public decimal TotalAmount { get; set; }
  public decimal TotalVat { get; set; }
  public decimal NetAmount { get; set; }
  public decimal TotalPaid { get; set; }
  public bool Paid { get; set; }
  public List<OrderItemDto> OrderItems { get; set; }
}

public class OrderItemDto : IDto
{
  public Guid Id { get; set; }
  public string ItemName { get; set; }
  public int Qty { get; set; }
  public decimal Price { get; set; }
  public decimal VatPercent { get; set; }
  public decimal VatAmount { get; set; }
  public decimal ItemTotal { get; set; }
}

public class OrderExportDto : IDto
{
  public string OrderNumber { get; set; }
  public DateTime OrderDate { get; set; }
  public string PhoneNumber { get; set; }
  public string CustomerName { get; set; }
  public decimal TotalAmount { get; set; }
  public decimal TotalVat { get; set; }
  public decimal NetAmount { get; set; }
  public decimal TotalPaid { get; set; }
  public bool Paid { get; set; }
  public List<OrderItemDto> OrderItems { get; set; }
}