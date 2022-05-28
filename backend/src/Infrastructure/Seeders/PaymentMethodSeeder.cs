using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class PaymentMethodSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<PaymentMethodSeeder> _logger;

  public PaymentMethodSeeder(ISerializerService serializerService, ILogger<PaymentMethodSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.02";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.PaymentMethods.Any())
    {
      _logger.LogInformation("Started to Seed PaymentMethods");

      // Here you can use your own logic to populate the database.
      // As an example, I am using a JSON file to populate the database.
      string productsData = await File.ReadAllTextAsync(path + "/Seeders/payment-methods.json", cancellationToken);
      var products = _serializerService.Deserialize<List<PaymentMethod>>(productsData);

      foreach (var product in products)
      {
        await _db.PaymentMethods.AddAsync(product, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded PaymentMethods");
    }
  }
}