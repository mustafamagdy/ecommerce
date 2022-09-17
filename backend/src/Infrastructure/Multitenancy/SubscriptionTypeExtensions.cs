using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public static class SubscriptionTypeExtensions
{
  public static void ValidateActiveSubscription(this SubscriptionType subscriptionType, DateTime now, FSHTenantInfo tenant)
  {
    switch (subscriptionType.Name)
    {
      case nameof(SubscriptionType.Standard):
        if (tenant.ProdSubscriptionId == null || !tenant.ProdSubscription!.IsActive(now))
          throw new UnauthorizedException($"Tenant {tenant.Id} has no active {subscriptionType.Name} subscription");

        break;
      case nameof(SubscriptionType.Demo):
        if (tenant.DemoSubscriptionId == null || !tenant.DemoSubscription!.IsActive(now))
          throw new UnauthorizedException($"Tenant {tenant.Id} has no active {subscriptionType.Name} subscription");

        break;
      case nameof(SubscriptionType.Train):
        if (tenant.TrainSubscriptionId == null || !tenant.TrainSubscription!.IsActive(now))
          throw new UnauthorizedException($"Tenant {tenant.Id} has no active {subscriptionType.Name} subscription");

        break;
    }
  }
}