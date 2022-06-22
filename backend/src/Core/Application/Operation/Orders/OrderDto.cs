namespace FSH.WebApi.Application.Operation.Orders;

public class OrderDto : IDto
{
  public Guid Id { get; set; }
  public string OrderNumber { get; set; } = default!;
  public string OrderDate { get; set; } = default!;
  public string OrderTime { get; set; } = default!;
  public string PhoneNumber { get; set; } = default!;
  public string CustomerName { get; set; } = default!;
  public decimal TotalAmount { get; set; }
  public decimal TotalVat { get; set; }
  public decimal NetAmount { get; set; }
  public decimal TotalPaid { get; set; }
  public bool Paid { get; set; }
  public List<OrderItemDto> OrderItems { get; set; } = default!;
}

public class OrderItemDto : IDto
{
  public Guid Id { get; set; }
  public string ItemName { get; set; } = default!;
  public int Qty { get; set; } = default!;
  public decimal Price { get; set; } = default!;
  public decimal VatPercent { get; set; }
  public decimal VatAmount { get; set; }
  public decimal ItemTotal { get; set; }
}

public class OrderExportDto : IDto
{
  public string OrderNumber { get; set; } = default!;
  public DateTime OrderDate { get; set; }
  public string PhoneNumber { get; set; } = default!;
  public string CustomerName { get; set; } = default!;
  public decimal TotalAmount { get; set; } = default!;
  public decimal TotalVat { get; set; }
  public decimal NetAmount { get; set; }
  public decimal TotalPaid { get; set; }
  public bool Paid { get; set; }
  public string Base64QrCode { get; set; } = default!;
  public List<OrderItemDto> OrderItems { get; set; } = default!;
}