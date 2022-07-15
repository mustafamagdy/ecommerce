using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSH.WebApi.Infrastructure.Multitenancy
{
  public class HasValidSubscriptionTypeFilter : IAsyncActionFilter
  {
    private readonly ITenantResolver _tenantResolver;
    private readonly ITenantService _tenantService;
    private readonly ISystemTime _systemTime;

    public HasValidSubscriptionTypeFilter(ITenantResolver tenantResolver, ITenantService tenantService, ISystemTime systemTime)
    {
      _tenantResolver = tenantResolver;
      _tenantService = tenantService;
      _systemTime = systemTime;
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
        var hasSubscription = typeof(HasValidSubscriptionTypeAttribute);
        var controllerInfo = descriptor.ControllerTypeInfo;

        var hasControllerLevel = controllerInfo.GetCustomAttributes(hasSubscription, true).Length > 0;
        var hasMethodLevel = descriptor.MethodInfo.GetCustomAttributes(hasSubscription, true).Length > 0;

        if (hasControllerLevel || hasMethodLevel)
        {
          var tenantContext = await _tenantResolver.ResolveAsync(context.HttpContext) as MultiTenantContext<FSHTenantInfo> ?? throw new FeatureNotAllowedException();
          var tenant = await _tenantService.GetByIdAsync(tenantContext.TenantInfo?.Id) ?? throw new FeatureNotAllowedException();
          if (tenant.Id != MultitenancyConstants.Root.Id)
          {
            if (tenant.ProdSubscriptionId == null && tenant.DemoSubscriptionId == null && tenant.TrainSubscriptionId == null)
            {
              throw new FeatureNotAllowedException();
            }

            var subscription = tenant.ProdSubscription;
            if (subscription.ExpiryDate < _systemTime.Now)
            {
              throw new SubscriptionExpiredException(subscription.ExpiryDate, "Subscription expired");
            }
          }
        }

        await next();
      }
    }
  }

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class HasValidSubscriptionTypeAttribute : Attribute
  {
    public SubscriptionType Type { get; }

    public HasValidSubscriptionTypeAttribute(SubscriptionType type)
    {
      Type = type;
    }

    public HasValidSubscriptionTypeAttribute(string type)
    {
      // Type = type;
    }
  }

  public class AllowWithNoSubscriptionAttribute : Attribute
  {
  }
}