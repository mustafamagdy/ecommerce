using System.ComponentModel.DataAnnotations.Schema;

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

public class OrderSummaryReportDto : IDto
{
  public string DateFrom { get; set; }
  public string DateTo { get; set; }
  public decimal TotalAmount { get; set; }
  public decimal TotalPaid { get; set; }
  public decimal TotalVat { get; set; }
  public decimal TotalRemaining { get; set; }

  public List<OrderSummaryDto> Orders { get; set; }
}

public class OrderSummaryDto : IDto
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
}

public class OrderExportDto : OrderSummaryDto
{
  public OrderExportDto()
  {
  }

  public string Base64QrCode { get; set; }
  public List<OrderItemDto> OrderItems { get; set; }
}