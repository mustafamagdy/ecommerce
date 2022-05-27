using FSH.WebApi.Domain.Catalog;

namespace FSH.WebApi.Domain.Operation;

public class OrderItem : BaseEntity, IAggregateRoot
{
  public string ProductName { get; private set; }
  public string ServiceName { get; private set; }
  public decimal Price { get; private set; }
  public decimal VatPercent { get; private set; }
  public decimal VatAmount => Price * VatPercent;
  public decimal ItemTotal => Price * VatAmount;
  public string ItemName => $"{ServiceName} - {ProductName}";

  public Guid OrderId { get; private set; }
  public virtual Order Order { get; private set; }
  public Guid ServiceCatalogId { get; private set; }
  public virtual ServiceCatalog ServiceCatalog { get; private set; }

  public OrderItem(string serviceName, string productName, Guid serviceCatalogId, decimal price, decimal vatPercent, Guid orderId)
  {
    ServiceName = serviceName;
    ProductName = productName;
    ServiceCatalogId = serviceCatalogId;
    Price = price;
    VatPercent = vatPercent;
    OrderId = orderId;
  }

  public OrderItem Update(string? serviceName, string? productName, Guid? serviceCatalogId, decimal? price, decimal? vatPercent)
  {
    if (serviceName is not null && !ServiceName.Equals(serviceName)) ServiceName = serviceName;
    if (productName is not null && !ProductName.Equals(productName)) ProductName = productName;
    if (serviceCatalogId is not null && !ServiceCatalogId.Equals(serviceCatalogId.Value)) ServiceCatalogId = serviceCatalogId.Value;
    if (price is not null && !Price.Equals(price.Value)) Price = price.Value;
    if (vatPercent is not null && !VatPercent.Equals(vatPercent.Value)) VatPercent = vatPercent.Value;

    return this;
  }
}