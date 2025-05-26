using Mapster;

namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceDto :IRegister, IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public void Register(TypeAdapterConfig config)
  {
      config.NewConfig<Service, ServiceDto>()
          .Map(s => s.ImageUrl, s => s.ImagePath);
  }
}