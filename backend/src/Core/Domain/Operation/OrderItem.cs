using FSH.WebApi.Domain.Catalog;

namespace FSH.WebApi.Domain.Operation;

public class OrderItem : BaseEntity, IAggregateRoot
{
  public OrderItem(Guid serviceCatalogId, int qty, decimal price, string productName, string serviceName, decimal vatPercent)
  {
    ServiceCatalogId = serviceCatalogId;
    Qty = qty;
    Price = price;
    VatPercent = vatPercent;
    ProductName = productName;
    ServiceName = serviceName;
  }

  public string ProductName { get; private set; }
  public string ServiceName { get; private set; }
  public int Qty { get; private set; }
  public decimal Price { get; private set; }
  public decimal VatPercent { get; private set; }
  public decimal VatAmount => (Qty * Price) * VatPercent;
  public decimal ItemTotal => (Qty * Price) + VatAmount;
  public string ItemName => $"{ServiceName} - {ProductName}";

  public Guid OrderId { get; private set; }
  public Order Order { get; private set; }
  public Guid ServiceCatalogId { get; private set; }
  public ServiceCatalog ServiceCatalog { get; private set; }

  public OrderItem SetOrderId(Guid orderId)
  {
    OrderId = orderId;
    return this;
  }
}