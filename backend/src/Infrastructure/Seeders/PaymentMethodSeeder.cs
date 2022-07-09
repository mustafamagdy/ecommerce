using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Multitenancy;
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

  public string Order => "02.05";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.PaymentMethods.Any())
    {
      _logger.LogInformation("Started to Seed PaymentMethods");

      string jsonData = await File.ReadAllTextAsync(path + "/Seeders/payment-methods.json", cancellationToken);
      var items = _serializerService.Deserialize<List<PaymentMethod>>(jsonData);

      foreach (var item in items)
      {
        await _db.PaymentMethods.AddAsync(item, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded PaymentMethods");
    }
  }
}

public class RootPaymentMethodSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly TenantDbContext _db;
  private readonly ILogger<RootPaymentMethodSeeder> _logger;

  public RootPaymentMethodSeeder(ISerializerService serializerService, ILogger<RootPaymentMethodSeeder> logger, TenantDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "01.02";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.RootPaymentMethods.Any())
    {
      _logger.LogInformation("(Tenant DB) Started to Seed PaymentMethods");

      string jsonData = await File.ReadAllTextAsync(path + "/Seeders/payment-methods.json", cancellationToken);
      var items = _serializerService.Deserialize<List<PaymentMethod>>(jsonData);

      foreach (var item in items)
      {
        await _db.RootPaymentMethods.AddAsync(item, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("(Tenant DB) Seeded PaymentMethods");
    }
  }
}