namespace FSH.WebApi.Shared.Multitenancy;

public interface ISubscriptionTypeResolver
{
  SubscriptionType Resolve();
}