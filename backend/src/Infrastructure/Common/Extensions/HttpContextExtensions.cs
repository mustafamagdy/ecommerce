using FSH.WebApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Common.Extensions;

public static class HttpContextExtensions
{
  public static ManagerOverrideToken GetManagerOverrideToken(this HttpContext context)
  {
    return new ManagerOverrideToken("Permissions.Brands.Create", new { Name = "brand11" });

    // return new ManagerOverrideToken
    // {
    //   Permission = "Permissions.Brands.Delete",
    //   Scope = "08da48b0-515f-478e-8681-e99a622f6985"
    // };
  }
}