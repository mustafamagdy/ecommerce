using Mapster;

namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public class ServiceCatalogDto :IRegister, IDto
{
  public Guid Id { get; set; }
  public string ServiceName { get; set; } = default!;
  public string? ServiceImageUrl { get; set; }
  public Guid ProductId { get; set; } = default!;
  public string ProductName { get; set; } = default!;
  public Guid? CategoryId { get; set; } = default!;
  public string CategoryName { get; set; } = default!;
  public string? ProductImageUrl { get; set; }
  public decimal Price { get; set; }
  public void Register(TypeAdapterConfig config)
  {
      config.NewConfig<ServiceCatalog, ServiceCatalogDto>()
          .Map(dis => dis.ProductId, src => src.Product.Id!)
          .Map(dis => dis.ProductImageUrl, src => src.Product.ImagePath!)
          .Map(dis => dis.ServiceImageUrl, src => src.Service.ImagePath!);
  }
}