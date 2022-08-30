using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public interface ISubscriptionAccessor
{
  SubscriptionType SubscriptionType { get; set; }
}

public class SubscriptionAccessor : ISubscriptionAccessor
{
  private SubscriptionType _subscriptionType;

  public SubscriptionAccessor(IHttpContextAccessor httpContextAccessor, ITenantInfo? currentTenant)
  {
    _subscriptionType = GetSubscriptionType(httpContextAccessor, currentTenant);
  }

  private SubscriptionType GetSubscriptionType(IHttpContextAccessor httpContextAccessor, ITenantInfo? currentTenant)
  {
    if (currentTenant == null || currentTenant.Id == MultitenancyConstants.Root.Id)
    {
      return SubscriptionType.Standard;
    }

    var context = httpContextAccessor.HttpContext;
    if (context == null)
    {
      // throw new ArgumentException("Tenant subscription cannot be resolved");
      return SubscriptionType.Standard;
    }

    var headerValue = context.Request.Headers.FirstOrDefault(a => a.Key == MultitenancyConstants.SubscriptionTypeHeaderName);
    if (headerValue.Value.Count > 0 && SubscriptionType.TryFromValue(headerValue.Value[0], out var subscriptionType))
    {
      return subscriptionType;
    }

    // throw new MissingHeaderException("Tenant subscription type not found in request header");
    return SubscriptionType.Standard;
  }

  public SubscriptionType SubscriptionType { get => _subscriptionType; set => _subscriptionType = value; }
}