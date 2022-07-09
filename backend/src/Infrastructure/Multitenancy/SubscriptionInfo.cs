using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public interface ISubscriptionInfo
{
  public SubscriptionType SubscriptionType { get; }
}

public class SubscriptionInfo : ISubscriptionInfo
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public SubscriptionInfo(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }


  //TODO: get from the subdomain, if we can add strategies
  public SubscriptionType SubscriptionType => SubscriptionType.Standard;
}