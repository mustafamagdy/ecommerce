using System.Security.Claims;
using FSH.WebApi.Application.Identity.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;


namespace FSH.WebApi.Infrastructure.Auth.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  private readonly IUserService _userService;

  public PermissionAuthorizationHandler(IUserService userService) =>
    _userService = userService;

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
    PermissionRequirement requirement)
  {
    var filterContext = context.Resource as AuthorizationFilterContext;
    var routeInfo = context.Resource as RouteEndpoint;
    var response = filterContext?.HttpContext.Response;

    // var routeValues = (context.Resource as HttpContext).Request.RouteValues;
    // var controllerName = routeValues["controller"].ToString();
    // var actionName = routeValues["action"].ToString();

    // var mvcContext = context.Resource as AuthorizationFilterContext;
    // if (mvcContext?.ActionDescriptor is ControllerActionDescriptor descriptor)
    // {
    //   var actionName = descriptor.ActionName;
    //   var ctrlName = descriptor.ControllerName;
    // }

    if (context.User?.GetUserId() is { } userId &&
        await _userService.HasPermissionAsync(userId, requirement.Permission))
    {
      context.Succeed(requirement);
    }
  }
}