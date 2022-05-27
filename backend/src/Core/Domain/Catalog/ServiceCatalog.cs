using System.ComponentModel;

namespace FSH.WebApi.Domain.Catalog;

public enum ServicePriority
{
  [Description("normal")] Normal,
  [Description("urgent")] Urgent
}

public class ServiceCatalog : AuditableEntity, IAggregateRoot
{
  public Guid ServiceId { get; set; }
  public Service Service { get; set; }

  public Guid ProductId { get; set; }
  public Product Product { get; set; }

  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }

  public ServiceCatalog(Guid serviceId, Guid productId, decimal price, ServicePriority priority)
  {
    ServiceId = serviceId;
    ProductId = productId;
    Price = price;
    Priority = priority;
  }

  public ServiceCatalog Update(decimal price, ServicePriority priority)
  {
    Price = price;
    Priority = priority;
    return this;
  }
}