using Ardalis.Specification;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Multitenancy;


public class SubscriptionTypeResolver : ISubscriptionTypeResolver
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
  private readonly ITenantInfo _currentTenant;
  private readonly ISystemTime _systemTime;

  public SubscriptionTypeResolver(IHttpContextAccessor httpContextAccessor, ITenantInfo? currentTenant,
    IReadNonAggregateRepository<FSHTenantInfo> tenantRepo, ISystemTime systemTime)
  {
    _httpContextAccessor = httpContextAccessor;
    _currentTenant = currentTenant;
    _tenantRepo = tenantRepo;
    _systemTime = systemTime;
  }

  public SubscriptionType Resolve()
  {
    if (_currentTenant == null || _currentTenant.Id == MultitenancyConstants.Root.Id)
    {
      return SubscriptionType.Standard;
    }

    var context = _httpContextAccessor.HttpContext;
    if (context == null)
    {
      // return SubscriptionType.Standard;
      throw new ArgumentException("Tenant subscription cannot be resolved");
    }

    var headerValue = context.Request.Headers.FirstOrDefault(a => a.Key == MultitenancyConstants.SubscriptionTypeHeaderName);
    if (headerValue.Value.Count <= 0 || !SubscriptionType.TryFromValue(headerValue.Value[0], out var subscriptionType))
    {
      throw new MissingHeaderException("Tenant subscription type not found in request header");
    }

    var tenant = _tenantRepo.FirstOrDefaultAsync(new SingleResultSpecification<FSHTenantInfo>()
        .Query
        .Include(a => a.ProdSubscription)
        .Include(a => a.DemoSubscription)
        .Include(a => a.TrainSubscription)
        .Where(a => a.Id == _currentTenant.Id)
        .Specification)
      .GetAwaiter().GetResult();

    subscriptionType.ValidateActiveSubscription(_systemTime.Now, tenant);

    return subscriptionType;
  }
}