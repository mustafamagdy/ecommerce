using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public interface ISubscriptionResolver : ITransientService
{
  public SubscriptionType Resolve(string tenantId);
}

public class SubscriptionResolver : ISubscriptionResolver
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public SubscriptionResolver(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public SubscriptionType Resolve(string tenantId)
  {
    if (tenantId == MultitenancyConstants.Root.Id)
    {
      return SubscriptionType.Standard;
    }

    var context = _httpContextAccessor.HttpContext;
    if (context == null)
    {
      throw new ArgumentException("Tenant subscription cannot be resolved");
    }

    var headerValue = context.Request.Headers.FirstOrDefault(a => a.Key == MultitenancyConstants.SubscriptionTypeHeaderName);
    if (headerValue.Value.Count > 0 && SubscriptionType.TryFromValue(headerValue.Value[0], out var subscriptionType))
    {
      return subscriptionType;
    }

    throw new MissingHeaderException("Tenant subscription type not found in request header");
  }
}