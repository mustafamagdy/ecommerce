using System.ComponentModel;
using System.Runtime.Serialization;
using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.Catalog;

[JsonConverter(typeof(SmartEnumNameConverter<ServicePriority, string>))]
public class ServicePriority : SmartEnum<ServicePriority, string>
{
  public static readonly ServicePriority Normal = new(nameof(Normal), "normal");
  public static readonly ServicePriority Urgent = new(nameof(Urgent), "urgent");

  public ServicePriority(string name, string value)
    : base(name, value)
  {
  }
}

public class ServiceCatalog : AuditableEntity, IAggregateRoot
{
  public ServiceCatalog(Guid serviceId, Guid productId, decimal price, ServicePriority priority)
  {
    ServiceId = serviceId;
    ProductId = productId;
    Price = price;
    Priority = priority;
  }

  public Guid ServiceId { get; private set; }
  public Service Service { get; private set; }

  public Guid ProductId { get; private set; }
  public Product Product { get; private set; }

  public decimal Price { get; private set; }
  public ServicePriority Priority { get; private set; }

  public ServiceCatalog Update(decimal price, ServicePriority priority)
  {
    Price = price;
    Priority = priority;
    return this;
  }
}