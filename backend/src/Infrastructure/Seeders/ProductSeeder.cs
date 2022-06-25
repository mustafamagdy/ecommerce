using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class ProductSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<ProductSeeder> _logger;

  public ProductSeeder(ISerializerService serializerService, ILogger<ProductSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.04";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.Products.Any())
    {
      _logger.LogInformation("Started to Seed Products");

      // Here you can use your own logic to populate the database.
      // As an example, I am using a JSON file to populate the database.
      string productsData = await File.ReadAllTextAsync(path + "/Seeders/products.json", cancellationToken);
      var products = _serializerService.Deserialize<List<Product>>(productsData);

      foreach (var product in products)
      {
        await _db.Products.AddAsync(product, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded Products");
    }
  }
}