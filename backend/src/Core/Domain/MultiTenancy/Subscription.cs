using Ardalis.SmartEnum;

namespace FSH.WebApi.Domain.MultiTenancy;

public abstract class Subscription : BaseEntity
{
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
    SubscriptionHistory = new HashSet<SubscriptionHistory>();
  }

  public HashSet<SubscriptionHistory> SubscriptionHistory { get; set; }
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

public class SubscriptionType : SmartEnum<SubscriptionType, string>
{
  public static readonly SubscriptionType Standard = new(nameof(Standard), "std");
  public static readonly SubscriptionType Demo = new(nameof(Demo), "demo");
  public static readonly SubscriptionType Train = new(nameof(Train), "train");

  public SubscriptionType(string name, string value)
    : base(name, value)
  {
  }
}