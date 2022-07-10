using Finbuckle.MultiTenant;
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

    public HasValidSubscriptionTypeFilter(ITenantResolver tenantResolver, ITenantService tenantService)
    {
      _tenantResolver = tenantResolver;
      _tenantService = tenantService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
      if (descriptor?.MethodInfo
            .GetCustomAttributes(typeof(HasValidSubscriptionTypeAttribute), true)
            .ToList()
            .FirstOrDefault() is HasValidSubscriptionTypeAttribute subscriptionLevel)
      {
        if (await _tenantResolver.ResolveAsync(context.HttpContext) is MultiTenantContext<FSHTenantInfo> tenantContext)
        {
          var tenant = await _tenantService.GetByIdAsync(tenantContext.TenantInfo?.Id);
          if (tenant.ProdSubscription == null)
          {
            throw new FeatureNotAllowedException();
          }
        }

        await next();
      }

      await next();
    }
  }

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
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
}