using Finbuckle.MultiTenant;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public interface ISubscriptionResolver
{
  public SubscriptionType Resolve();
}

public class SubscriptionResolver : ISubscriptionResolver
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ITenantInfo _currentTenant;

  public SubscriptionResolver(IHttpContextAccessor httpContextAccessor, ITenantInfo currentTenant)
  {
    _httpContextAccessor = httpContextAccessor;
    _currentTenant = currentTenant;
  }

  public SubscriptionType Resolve()
  {
    if (_currentTenant.Id == MultitenancyConstants.Root.Id)
    {
      return SubscriptionType.Standard;
    }

    var context = _httpContextAccessor.HttpContext;
    if (context == null)
    {
      throw new ArgumentException("Tenant subscription cannot be resolved");
    }

    var claimValue = context.User?.Claims.FirstOrDefault(c => c.Type == FSHClaims.Subscription);
    if (claimValue == null)
    {
      throw new MissingHeaderException("Tenant subscription not found in user claims");
    }

    if (!SubscriptionType.TryFromValue(claimValue.Value, out var subscriptionType))
    {
      throw new MissingHeaderException("Tenant subscription not found in user claims");
    }

    return subscriptionType;
  }
}