using FSH.WebApi.Domain.Catalog;

namespace FSH.WebApi.Domain.Operation;

public class OrderItem : BaseEntity, IAggregateRoot
{
  public string ItemName { get; private set; }
  public decimal Price { get; private set; }
  public decimal VatPercent { get; private set; }
  public decimal VatAmount => Price * VatPercent;
  public decimal ItemTotal => Price * VatAmount;

  public Guid OrderId { get; private set; }
  public virtual Order Order { get; private set; }
  public Guid ServiceCatalogId { get; private set; }
  public virtual ServiceCatalog ServiceCatalog { get; private set; }

  public OrderItem(string itemName, Guid serviceCatalogId, decimal price, decimal vatPercent, Guid orderId)
  {
    ItemName = itemName;
    ServiceCatalogId = serviceCatalogId;
    Price = price;
    VatPercent = vatPercent;
    OrderId = orderId;
  }

  public OrderItem Update(string? itemName, Guid? serviceCatalogId, decimal? price, decimal? vatPercent)
  {
    if (itemName is not null && !ItemName.Equals(itemName)) ItemName = itemName;
    if (serviceCatalogId is not null && !ServiceCatalogId.Equals(serviceCatalogId.Value)) ServiceCatalogId = serviceCatalogId.Value;
    if (price is not null && !Price.Equals(price.Value)) Price = price.Value;
    if (vatPercent is not null && !VatPercent.Equals(vatPercent.Value)) VatPercent = vatPercent.Value;

    return this;
  }
}