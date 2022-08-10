namespace FSH.WebApi.Domain.Operation;

public class Order : AuditableEntity, IAggregateRoot
{
  private readonly List<OrderItem> _orderItems = new();
  private readonly List<OrderPayment> _orderPayments = new();

  public Order(Guid customerId, string orderNumber, DateTime orderDate)
  {
    CustomerId = customerId;
    OrderNumber = orderNumber;
    OrderDate = orderDate;
  }

  public string OrderNumber { get; private set; }
  public Guid CustomerId { get; private set; }
  public DateTime OrderDate { get; private set; }
  public string QrCodeBase64 { get; private set; }
  public Customer Customer { get; private set; }

  public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
  public IReadOnlyList<OrderPayment> OrderPayments => _orderPayments.AsReadOnly();

  public decimal TotalAmount => OrderItems?.Sum(a => a.Price * a.Qty) ?? 0;
  public decimal TotalVat => OrderItems?.Sum(a => a.VatAmount) ?? 0;
  public decimal NetAmount => OrderItems?.Sum(a => a.ItemTotal) ?? 0;
  public decimal TotalPaid => OrderPayments?.Sum(a => a.Amount) ?? 0;
  public bool Paid => TotalPaid >= NetAmount;

  public void SetInvoiceQrCode(string? invoiceQrCode)
  {
    if (invoiceQrCode is not null && (string.IsNullOrEmpty(QrCodeBase64) || !QrCodeBase64.Equals(invoiceQrCode)))
      QrCodeBase64 = invoiceQrCode;
  }

  public void AddItem(OrderItem item)
  {
    item.SetOrderId(Id);
    _orderItems.Add(item);
  }

  public void AddItems(List<OrderItem> items)
  {
    items.ForEach(a => a.SetOrderId(Id));
    _orderItems.AddRange(items);
  }

  public void AddPayment(OrderPayment payment) => _orderPayments.Add(payment);
}