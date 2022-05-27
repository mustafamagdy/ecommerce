using FSH.WebApi.Application.Catalog.ServiceCategories;

namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceDetailsDto : IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImagePath { get; set; }
  public ServiceCategoryDto ServiceCategory { get; set; } = default!;
}