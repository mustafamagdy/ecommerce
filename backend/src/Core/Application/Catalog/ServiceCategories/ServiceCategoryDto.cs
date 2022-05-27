namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class ServiceCategoryDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconUrl { get; set; }
}