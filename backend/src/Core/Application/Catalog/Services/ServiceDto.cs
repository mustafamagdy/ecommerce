namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceDto : IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
}