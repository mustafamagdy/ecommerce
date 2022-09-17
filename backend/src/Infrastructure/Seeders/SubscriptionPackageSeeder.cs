using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FSH.WebApi.Infrastructure.Seeders;

public sealed class SubscriptionPackageSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly TenantDbContext _db;
  private readonly ILogger<SubscriptionPackageSeeder> _logger;

  public SubscriptionPackageSeeder(ISerializerService serializerService, ILogger<SubscriptionPackageSeeder> logger, TenantDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "02.02";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    bool hasPackages = _db.Packages.Any();
    if (hasPackages)
    {
      return;
    }

    _logger.LogInformation("Started to Seed Subscription Packages");

    string jsonData = await File.ReadAllTextAsync(path + "/Seeders/subscription-packages.json", cancellationToken);
    var items = JsonConvert.DeserializeObject<List<SubscriptionPackage>>(jsonData);

    await _db.Packages.AddRangeAsync(items, cancellationToken);

    await _db.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Seeded Subscription");
  }
}