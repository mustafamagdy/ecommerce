using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace FSH.WebApi.Shared.Multitenancy;

[JsonConverter(typeof(SmartEnumNameConverter<SubscriptionFeatureType, string>))]
public class SubscriptionFeatureType : SmartEnum<SubscriptionFeatureType, string>
{
  public static readonly SubscriptionFeatureType NoOfUsers = new(nameof(NoOfUsers), "no_of_users");
  public static readonly SubscriptionFeatureType OrdersPerMonth = new(nameof(OrdersPerMonth), "orders_per_month");

  public SubscriptionFeatureType(string name, string value)
    : base(name, value)
  {
  }
}

[JsonConverter(typeof(SmartEnumNameConverter<SubscriptionType, string>))]
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