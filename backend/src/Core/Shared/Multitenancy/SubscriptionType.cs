using Ardalis.SmartEnum;

namespace FSH.WebApi.Shared.Multitenancy;

public class SubscriptionType : SmartEnum<SubscriptionType, string>
{
  public const string StandardValue = "stf";
  public static readonly SubscriptionType Standard = new(nameof(Standard), StandardValue);
  public static readonly SubscriptionType Demo = new(nameof(Demo), "demo");
  public static readonly SubscriptionType Train = new(nameof(Train), "train");

  public SubscriptionType(string name, string value)
    : base(name, value)
  {
  }
}