namespace FSH.WebApi.Application.Catalog.Categories;

public class CategoryDetailsDto : IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}