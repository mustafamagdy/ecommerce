namespace FSH.WebApi.Application.Catalog.Products;

public class CategorytExportDto : IDto
{
  public string Name { get; set; } = default!;
  public string Description { get; set; } = default!;
}