using System.Security.Claims;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using ObjectsComparer;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace FSH.WebApi.Infrastructure.Auth.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  private readonly IUserService _userService;
  private readonly IOverrideTokenService _overrideTokenService;
  public PermissionAuthorizationHandler(IUserService userService, IOverrideTokenService overrideTokenService)
  {
    _userService = userService;
    _overrideTokenService = overrideTokenService;
  }

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
        var request = ctx.Request;
        var motToken = request.Headers["mot"];
        if (string.IsNullOrEmpty(motToken))
        {
          context.Fail();
          return;
        }

        var mot = _overrideTokenService.ExtractManagerOverrideTokenValues(motToken);
        if (mot.Permission == requirement.Permission)
        {
          var endpoint = ctx.GetEndpoint();
          var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

          var comparer = new Comparer(new ComparisonSettings
          {
            RecursiveComparison = true,
            EmptyAndNullEnumerablesEqual = true
          });

          if (actionDescriptor!.Parameters.Count < 1)
          {
            throw new NotSupportedException("MOT not supported for parameterless operation");
          }

          var p1 = actionDescriptor!.Parameters.FirstOrDefault();
          var p1Type = p1.ParameterType;

          if (p1Type.IsPrimitive || p1Type == typeof(string) || p1Type == typeof(Guid))
          {
            string? routeValue = request.RouteValues[p1.Name]?.ToString();
            object? scopedValue = JsonSerializer.Deserialize(mot.Scope, p1Type);
            if (comparer.Compare(routeValue, scopedValue))
            {
              context.Succeed(requirement);
            }
          }
          else
          {
            request.EnableBuffering();
            var reader = new StreamReader(request.BodyReader.AsStream());
            string body = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);

            object? model = JsonConvert.DeserializeObject(body, p1Type);
            if (comparer.Compare(model, mot.Scope))
            {
              context.Succeed(requirement);
            }
          }
        }
      }
    }
  }
}