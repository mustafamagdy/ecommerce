using System.Security.Claims;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using ObjectsComparer;


namespace FSH.WebApi.Infrastructure.Auth.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  private readonly IUserService _userService;

  public PermissionAuthorizationHandler(IUserService userService) =>
    _userService = userService;

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
    PermissionRequirement requirement)
  {
    if (context.User?.GetUserId() is { } userId &&
        await _userService.HasPermissionAsync(userId, requirement.Permission))
    {
      context.Succeed(requirement);
    }
    else
    {
      if (context.Resource is HttpContext ctx)
      {
        var mot = ctx.GetManagerOverrideToken();
        if (mot.Permission == requirement.Permission)
        {
          var endpoint = ctx.GetEndpoint();
          var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

          var request = ctx.Request;

          var p1 = actionDescriptor.Parameters.FirstOrDefault();
          var p1Type = p1.ParameterType;

          if (p1Type.IsPrimitive || p1Type == typeof(string) || p1Type == typeof(Guid))
          {
            string routeValue = request.RouteValues[p1.Name].ToString();
            if (routeValue.Equals(mot.Scope))
            {
              context.Succeed(requirement);
            }
          }
          else
          {
            request.EnableBuffering();

            var reader = new StreamReader(request.BodyReader.AsStream());
            string body = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject(body, p1Type);
            request.Body.Seek(0, SeekOrigin.Begin);
            // request.Body.Seek(0, SeekOrigin.Begin);
            var comparer = new Comparer(new ComparisonSettings
            {
              RecursiveComparison = true,
              EmptyAndNullEnumerablesEqual = true
            });
            if (comparer.Compare(model, mot.Scope))
            {
              context.Succeed(requirement);
            }

            request.BodyReader.AdvanceTo(new SequencePosition());
            // if (model.Equals(mot.Scope))
            // {
            //   context.Succeed(requirement);
            // }
          }
        }
      }
    }
  }
}