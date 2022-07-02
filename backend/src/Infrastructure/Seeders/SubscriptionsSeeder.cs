using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using FSH.WebApi.Shared.Multitenancy;
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
    bool hasStandardSubs = _db.StandardSubscriptions.Any();
    bool hasDemoSubs = _db.DemoSubscriptions.Any();
    bool hasTrainSubs = _db.TrainSubscriptions.Any();

    if (hasStandardSubs && hasDemoSubs && hasTrainSubs)
    {
      return;
    }

    _logger.LogInformation("Started to Seed Subscriptions");

    string jsonData = await File.ReadAllTextAsync(path + "/Seeders/subscriptions.json", cancellationToken);
    var items = _serializerService.Deserialize<List<Subscription>>(jsonData);
    var prod = items.Where(a => a.SubscriptionType == SubscriptionType.Standard).Cast<StandardSubscription>().SingleOrDefault();
    var demo = items.Where(a => a.SubscriptionType == SubscriptionType.Demo).Cast<DemoSubscription>().SingleOrDefault();
    var train = items.Where(a => a.SubscriptionType == SubscriptionType.Train).Cast<TrainSubscription>().SingleOrDefault();

    if (!hasStandardSubs && prod != null)
    {
      await _db.StandardSubscriptions.AddAsync(prod, cancellationToken);
    }

    if (!hasDemoSubs && demo != null)
    {
      await _db.DemoSubscriptions.AddAsync(demo, cancellationToken);
    }

    if (!hasTrainSubs && train != null)
    {
      await _db.TrainSubscriptions.AddAsync(train, cancellationToken);
    }

    await _db.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Seeded Subscription");
  }
}