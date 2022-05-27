using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Catalog;

public class ServiceCatalogSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<ServiceCatalogSeeder> _logger;

  public ServiceCatalogSeeder(ISerializerService serializerService, ILogger<ServiceCatalogSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.04";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.ServiceCatalogs.Any())
    {
      _logger.LogInformation("Started to Seed ServiceCatalogs");

      // Here you can use your own logic to populate the database.
      // As an example, I am using a JSON file to populate the database.
      string serviceCategoryData = await File.ReadAllTextAsync(path + "/Catalog/services-catalog.json", cancellationToken);
      var serviceCategories = _serializerService.Deserialize<List<ServiceCatalog>>(serviceCategoryData);

      foreach (var serviceCategory in serviceCategories)
      {
        await _db.ServiceCatalogs.AddAsync(serviceCategory, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded ServiceCatalogs");
    }
  }
}