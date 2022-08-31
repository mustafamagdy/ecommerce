// using System.Reflection;
// using FSH.WebApi.Application.Common.Interfaces;
// using FSH.WebApi.Domain.MultiTenancy;
// using FSH.WebApi.Infrastructure.Multitenancy;
// using FSH.WebApi.Infrastructure.Persistence.Initialization;
// using FSH.WebApi.Shared.Multitenancy;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
//
// namespace FSH.WebApi.Infrastructure.Seeders;
//
// public class SubscriptionConverterWithSubscriptionType : JsonConverter
// {
//   public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//   {
//     JObject item = JObject.Load(reader);
//     var type = item["SubscriptionType"].Value<string>();
//     var typeVal = SubscriptionType.FromName(type);
//
//     Subscription subscription = typeVal.Name switch
//     {
//       nameof(SubscriptionType.Standard) => new StandardSubscription(),
//       nameof(SubscriptionType.Demo) => new DemoSubscription(),
//       nameof(SubscriptionType.Train) => new TrainSubscription(),
//       _ => throw new ArgumentOutOfRangeException(type)
//     };
//
//     serializer.Populate(item.CreateReader(), subscription);
//     return subscription;
//   }
//
//   public override bool CanConvert(Type objectType)
//   {
//     return typeof(Subscription).IsAssignableFrom(objectType);
//   }
//
//   public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//   {
//     throw new NotImplementedException();
//   }
// }
//
// public class SubscriptionSeeder : ICustomSeeder
// {
//   private readonly ISerializerService _serializerService;
//   private readonly TenantDbContext _db;
//   private readonly ILogger<SubscriptionSeeder> _logger;
//
//   public SubscriptionSeeder(ISerializerService serializerService, ILogger<SubscriptionSeeder> logger, TenantDbContext db)
//   {
//     _serializerService = serializerService;
//     _logger = logger;
//     _db = db;
//   }
//
//   public string Order => "02.02";
//
//   public async Task InitializeAsync(CancellationToken cancellationToken)
//   {
//     string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//     bool hasStandardSubs = _db.StandardSubscriptions.Any();
//     bool hasDemoSubs = _db.DemoSubscriptions.Any();
//     bool hasTrainSubs = _db.TrainSubscriptions.Any();
//
//     if (hasStandardSubs && hasDemoSubs && hasTrainSubs)
//     {
//       return;
//     }
//
//     _logger.LogInformation("Started to Seed Subscriptions");
//
//     string jsonData = await File.ReadAllTextAsync(path + "/Seeders/subscriptions.json", cancellationToken);
//     var items = JsonConvert.DeserializeObject<List<Subscription>>(jsonData, new SubscriptionConverterWithSubscriptionType());
//     var prod = items.OfType<StandardSubscription>().Single();
//     var demo = items.OfType<DemoSubscription>().Single();
//     var train = items.OfType<TrainSubscription>().Single();
//
//     if (!hasStandardSubs && prod != null)
//     {
//       await _db.StandardSubscriptions.AddAsync(prod, cancellationToken);
//     }
//
//     if (!hasDemoSubs && demo != null)
//     {
//       await _db.DemoSubscriptions.AddAsync(demo, cancellationToken);
//     }
//
//     if (!hasTrainSubs && train != null)
//     {
//       await _db.TrainSubscriptions.AddAsync(train, cancellationToken);
//     }
//
//     await _db.SaveChangesAsync(cancellationToken);
//     _logger.LogInformation("Seeded Subscription");
//   }
// }