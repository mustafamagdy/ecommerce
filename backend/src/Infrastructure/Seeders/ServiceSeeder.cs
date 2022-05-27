using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class ServiceSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<ServiceSeeder> _logger;

  public ServiceSeeder(ISerializerService serializerService, ILogger<ServiceSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.03";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.Services.Any())
    {
      _logger.LogInformation("Started to Seed Services");

      // Here you can use your own logic to populate the database.
      // As an example, I am using a JSON file to populate the database.
      string serviceCategoryData = await File.ReadAllTextAsync(path + "/Seeders/services.json", cancellationToken);
      var serviceCategories = _serializerService.Deserialize<List<Service>>(serviceCategoryData);

      foreach (var serviceCategory in serviceCategories)
      {
        await _db.Services.AddAsync(serviceCategory, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded Services");
    }
  }
}