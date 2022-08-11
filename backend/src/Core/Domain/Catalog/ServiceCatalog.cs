using System.ComponentModel;
using System.Runtime.Serialization;

namespace FSH.WebApi.Domain.Catalog;

public enum ServicePriority
{
  [EnumMember(Value = "normal")] Normal,
  [EnumMember(Value = "urgent")] Urgent
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