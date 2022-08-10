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
  public virtual Order Order { get; private set; }
  public Guid ServiceCatalogId { get; private set; }
  public virtual ServiceCatalog ServiceCatalog { get; private set; }

  public OrderItem Update(string? serviceName, string? productName, Guid? serviceCatalogId, int? qty, decimal? price, decimal? vatPercent)
  {
    if (serviceName is not null && !ServiceName.Equals(serviceName)) ServiceName = serviceName;
    if (productName is not null && !ProductName.Equals(productName)) ProductName = productName;
    if (serviceCatalogId is not null && !ServiceCatalogId.Equals(serviceCatalogId.Value)) ServiceCatalogId = serviceCatalogId.Value;
    if (qty is not null && !Qty.Equals(qty.Value)) Qty = qty.Value;
    if (price is not null && !Price.Equals(price.Value)) Price = price.Value;
    if (vatPercent is not null && !VatPercent.Equals(vatPercent.Value)) VatPercent = vatPercent.Value;

    return this;
  }

  public OrderItem SetOrderId(Guid orderId)
  {
    OrderId = orderId;
    return this;
  }
}