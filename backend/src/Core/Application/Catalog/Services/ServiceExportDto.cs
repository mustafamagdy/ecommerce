namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceExportDto : IDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ServiceCategoryName { get; set; } = default!;
}