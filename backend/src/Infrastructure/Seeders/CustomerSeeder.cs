using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class CustomerSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<CustomerSeeder> _logger;

  public CustomerSeeder(ISerializerService serializerService, ILogger<CustomerSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.06";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.Customers.Any())
    {
      _logger.LogInformation("Started to Seed Customers");

      // Here you can use your own logic to populate the database.
      // As an example, I am using a JSON file to populate the database.
      string productsData = await File.ReadAllTextAsync(path + "/Seeders/customers.json", cancellationToken);
      var products = _serializerService.Deserialize<List<Customer>>(productsData);

      foreach (var product in products)
      {
        await _db.Customers.AddAsync(product, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded Customers");
    }
  }
}