namespace FSH.WebApi.Domain.Operation;

public class Order : AuditableEntity, IAggregateRoot
{
  public string OrderNumber { get; private set; }
  public Guid CustomerId { get; private set; }
  public DateTime OrderDate { get; private set; }
  public string QrCodeBase64 { get; private set; }
  public virtual Customer Customer { get; private set; } = default!;
  public virtual HashSet<OrderItem> OrderItems { get; set; }
  public virtual HashSet<OrderPayment> OrderPayments { get; set; }

  public decimal TotalAmount => OrderItems?.Sum(a => a.Price * a.Qty) ?? 0;
  public decimal TotalVat => OrderItems?.Sum(a => a.VatAmount) ?? 0;
  public decimal NetAmount => OrderItems?.Sum(a => a.ItemTotal) ?? 0;
  public decimal TotalPaid => OrderPayments?.Sum(a => a.Amount) ?? 0;
  public bool Paid => TotalPaid >= NetAmount;

  public Order(Guid customerId, string orderNumber, DateTime orderDate)
  {
    CustomerId = customerId;
    OrderNumber = orderNumber;
    OrderDate = orderDate;
  }

  public Order Update(Guid? customerId)
  {
    if (customerId is not null && !CustomerId.Equals(customerId.Value)) CustomerId = customerId.Value;
    return this;
  }

  public Order SetInvoiceQrCode(string? invoiceQrCode)
  {
    if (invoiceQrCode is not null && !QrCodeBase64.Equals(invoiceQrCode)) QrCodeBase64 = invoiceQrCode;
    return this;
  }
}