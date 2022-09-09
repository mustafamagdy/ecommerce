using Ardalis.SmartEnum;

namespace FSH.WebApi.Domain.Operation;

public class OrderStatus : SmartEnum<OrderStatus, string>
{
  public static readonly OrderStatus Normal = new(nameof(Normal), "normal");
  public static readonly OrderStatus Canceled = new(nameof(Canceled), "canceled");

  public OrderStatus(string name, string value)
    : base(name, value)
  {
  }
}

public class Order : AuditableEntity, IAggregateRoot
{
  private readonly List<OrderItem> _orderItems = new();
  private readonly List<OrderPayment> _orderPayments = new();

  private Order()
  {
  }

  public Order(Customer customer, string orderNumber, DateTime orderDate)
  {
    OrderNumber = orderNumber;
    OrderDate = orderDate;
    Customer = customer;
  }

  public string OrderNumber { get; private set; }
  public DateTime OrderDate { get; private set; }
  public OrderStatus Status { get; set; } = OrderStatus.Normal;
  public string QrCodeBase64 { get; private set; }
  public Guid CustomerId { get; private set; }
  public Customer Customer { get; private set; }

  public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
  public IReadOnlyCollection<OrderPayment> OrderPayments => _orderPayments.AsReadOnly();

  public decimal TotalAmount { get; private set; }
  public decimal TotalVat { get; private set; }
  public decimal NetAmount { get; private set; }
  public decimal TotalPaid { get; private set; }
  public bool Paid => TotalPaid >= NetAmount;

  public void SetInvoiceQrCode(string? invoiceQrCode)
  {
    if (invoiceQrCode is not null && (string.IsNullOrEmpty(QrCodeBase64) || !QrCodeBase64.Equals(invoiceQrCode)))
      QrCodeBase64 = invoiceQrCode;
  }

  private void AddItem(OrderItem item)
  {
    item.SetOrderId(Id);
    _orderItems.Add(item);
    TotalAmount += item.Price * item.Qty;
    TotalVat += item.VatAmount;
    NetAmount += item.ItemTotal;
  }

  public void AddItems(List<OrderItem> items)
  {
    items.ForEach(AddItem);
  }

  public void AddPayment(OrderPayment payment)
  {
    Customer.PayDue(payment.Amount);
    _orderPayments.Add(payment);
    TotalPaid += payment.Amount;
  }

  public void Cancel(DateTime cancellationTime)
  {
    Status = OrderStatus.Canceled;
    Customer.PayDue(-1 * (TotalPaid - NetAmount));
  }
}