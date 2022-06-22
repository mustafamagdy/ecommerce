using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class SubscriptionSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly TenantDbContext _db;
  private readonly ILogger<SubscriptionSeeder> _logger;

  public SubscriptionSeeder(ISerializerService serializerService, ILogger<SubscriptionSeeder> logger, TenantDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.02";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.Subscriptions.Any())
    {
      _logger.LogInformation("Started to Seed Subscriptions");

      string jsonData = await File.ReadAllTextAsync(path + "/Seeders/subscriptions.json", cancellationToken);
      var items = _serializerService.Deserialize<List<Subscription>>(jsonData);

      foreach (var item in items)
      {
        await _db.Subscriptions.AddAsync(item, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded Subscriptions");
    }
  }
}