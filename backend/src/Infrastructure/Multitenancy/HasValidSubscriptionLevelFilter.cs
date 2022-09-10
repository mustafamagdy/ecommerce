using Ardalis.Specification;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSH.WebApi.Infrastructure.Multitenancy
{
  public class HasValidSubscriptionTypeFilter : IAsyncActionFilter
  {
    private readonly ITenantResolver _tenantResolver;
    private readonly ISystemTime _systemTime;
    private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
    private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;

    public HasValidSubscriptionTypeFilter(ITenantResolver tenantResolver,
      ISystemTime systemTime, IReadNonAggregateRepository<FSHTenantInfo> tenantRepo,
      ISubscriptionTypeResolver subscriptionTypeResolver)
    {
      _tenantResolver = tenantResolver;
      _systemTime = systemTime;
      _tenantRepo = tenantRepo;
      _subscriptionTypeResolver = subscriptionTypeResolver;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
      var allowWithNoSubscription = typeof(AllowWithNoSubscriptionAttribute);
      var hasSkipAtt = descriptor.MethodInfo.GetCustomAttributes(allowWithNoSubscription, true).Length > 0;
      if (hasSkipAtt)
      {
        await next();
      }
      else
      {
        var hasSubscription = typeof(HasValidSubscriptionAttribute);
        var controllerInfo = descriptor.ControllerTypeInfo;

        var hasControllerLevel = controllerInfo.GetCustomAttributes(hasSubscription, true).Length > 0;
        var hasMethodLevel = descriptor.MethodInfo.GetCustomAttributes(hasSubscription, true).Length > 0;

        if (hasControllerLevel || hasMethodLevel)
        {
          var tenantContext = await _tenantResolver.ResolveAsync(context.HttpContext) as MultiTenantContext<FSHTenantInfo>
                              ?? throw new UnauthorizedException("Unable to resolve tenant");

          string tenantId = tenantContext.TenantInfo?.Id!;
          if (tenantId == MultitenancyConstants.RootTenant.Id)
          {
            await next();
            return;
          }

          var tenant = await _tenantRepo.FirstOrDefaultAsync(
                           new SingleResultSpecification<FSHTenantInfo>()
                             .Query
                             .Include(a => a.ProdSubscription)
                             .Include(a => a.DemoSubscription)
                             .Include(a => a.TrainSubscription)
                             .Where(a => a.Id == tenantId).Specification)
                         .ConfigureAwait(false)
                       ?? throw new NotFoundException($"Tenant {tenantId} not found");

          if (!tenant.Active)
          {
            throw new UnauthorizedException($"Tenant {tenant.Id} is not active");
          }

          var subscriptionType = _subscriptionTypeResolver.Resolve();
          subscriptionType.ValidateActiveSubscription(_systemTime.Now, tenant);
        }

        await next();
      }
    }
  }

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class HasValidSubscriptionAttribute : Attribute
  {
  }

  public class AllowWithNoSubscriptionAttribute : Attribute
  {
  }
}