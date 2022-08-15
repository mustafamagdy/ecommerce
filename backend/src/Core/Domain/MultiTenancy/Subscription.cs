using Ardalis.SmartEnum.JsonNet;
using FSH.WebApi.Shared.Multitenancy;
using Newtonsoft.Json;

namespace FSH.WebApi.Domain.MultiTenancy;

public abstract class Subscription : BaseEntity
{
  public int Days { get; set; }
  public decimal Price { get; set; }
}

public class StandardSubscription : Subscription
{
  public StandardSubscription()
  {
    Days = 30;
    Price = 0.0m;
  }
}

public class DemoSubscription : Subscription
{
  public DemoSubscription()
  {
    Days = 365;
    Price = 0.0m;
  }
}

public class TrainSubscription : Subscription
{
  public TrainSubscription()
  {
    Days = 365;
    Price = 0.0m;
  }
}