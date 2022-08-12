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
  public OrderExportDto(string orderNumber, DateTime orderDate, string phoneNumber, string customerName, decimal totalAmount, decimal totalVat, decimal netAmount, decimal totalPaid, bool paid, string base64QrCode, List<OrderItemDto> orderItems)
  {
    OrderNumber = orderNumber;
    OrderDate = orderDate;
    PhoneNumber = phoneNumber;
    CustomerName = customerName;
    TotalAmount = totalAmount;
    TotalVat = totalVat;
    NetAmount = netAmount;
    TotalPaid = totalPaid;
    Paid = paid;
    Base64QrCode = base64QrCode;
    OrderItems = orderItems;
  }

  public string OrderNumber { get; }
  public DateTime OrderDate { get; }
  public string PhoneNumber { get; }
  public string CustomerName { get; }
  public decimal TotalAmount { get; }
  public decimal TotalVat { get; }
  public decimal NetAmount { get; }
  public decimal TotalPaid { get; }
  public bool Paid { get; }
  public string Base64QrCode { get; }
  public List<OrderItemDto> OrderItems { get; }
}