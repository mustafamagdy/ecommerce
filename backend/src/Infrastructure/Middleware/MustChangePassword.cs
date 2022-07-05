using System.Security.Claims;
using System.Web;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Middleware;

public class MustChangePasswordMiddleware : IMiddleware
{
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    if (context.User.Identity.IsAuthenticated &&
        context.Request.Path != new PathString("/personal/change-password") &&
        ((ClaimsIdentity)context.User.Identity).HasClaim(c => c.Type == FSHClaims.MustChangePassword))
    {
      throw new InvalidOperationException("user must change password first");
    }

    await next(context).ConfigureAwait(true);
  }
}