using Mapster;

namespace FSH.WebApi.Application.Catalog.Categories;

public class CategoryDto : IRegister, IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImagePath { get; set; }
  public Guid BrandId { get; set; }
  public string BrandName { get; set; } = default!;

  public void Register(TypeAdapterConfig config)
  {
    config.NewConfig<Category, CategoryDto>();
  }
}