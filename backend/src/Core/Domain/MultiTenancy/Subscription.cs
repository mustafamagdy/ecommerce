using Ardalis.SmartEnum.JsonNet;
using FSH.WebApi.Shared.Multitenancy;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.MultiTenancy;

public class Subscription : BaseEntity
{
  [JsonConverter(typeof(SmartEnumNameConverter<SubscriptionType, string>))]
  public SubscriptionType SubscriptionType { get; set; }

  public int Days { get; set; }
  public decimal Price { get; set; }
}

public class StandardSubscription : Subscription
{
  public StandardSubscription()
  {
    SubscriptionType = SubscriptionType.Standard;
    Days = 30;
    Price = 0.0m;
  }
}

public class DemoSubscription : Subscription
{
  public DemoSubscription()
  {
    SubscriptionType = SubscriptionType.Demo;
    Days = 365;
    Price = 0.0m;
  }
}

public class TrainSubscription : Subscription
{
  public TrainSubscription()
  {
    SubscriptionType = SubscriptionType.Train;
    Days = 365;
    Price = 0.0m;
  }
}