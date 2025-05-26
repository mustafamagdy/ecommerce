using System.ComponentModel;
using System.Runtime.Serialization;
using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.Catalog;

public sealed class ServiceCatalog : AuditableEntity, IAggregateRoot
{
  public ServiceCatalog(Guid serviceId, Guid productId, Guid? categoryId, decimal price)
  {
    ServiceId = serviceId;
    ProductId = productId;
    CategoryId = categoryId;
    Price = price;
  }

  public Guid ServiceId { get; private set; }
  public Service Service { get; private set; }

  public Guid ProductId { get; private set; }
  public Product Product { get; private set; }

  public Guid? CategoryId { get; private set; }
  public Category? Category { get; private set; }
  public decimal Price { get; private set; }

  public ServiceCatalog Update(Guid? productId, Guid? serviceId, Guid? categoryId, decimal? price)
  {
    if (productId.HasValue && productId.Value != Guid.Empty && !ProductId.Equals(productId.Value))
      ProductId = productId.Value;
    if (serviceId.HasValue && serviceId.Value != Guid.Empty && !ServiceId.Equals(serviceId.Value))
      ServiceId = serviceId.Value;
    if (categoryId.HasValue && categoryId.Value != Guid.Empty && !CategoryId.Equals(categoryId.Value))
      CategoryId = categoryId.Value;
    if (price.HasValue && Price != price) Price = price.Value;

    return this;
  }
}