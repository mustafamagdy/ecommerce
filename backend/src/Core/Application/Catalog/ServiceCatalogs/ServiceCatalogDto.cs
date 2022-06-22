namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class ServiceCatalogDto : IDto
{
  public Guid Id { get; set; }
  public string ServiceName { get; set; } = default!;
  public string? ServiceImageUrl { get; set; }
  public string ProductName { get; set; } = default!;
  public string? ProductImageUrl { get; set; }
  public decimal Price { get; set; }
  public ServicePriority Priority { get; set; }
}