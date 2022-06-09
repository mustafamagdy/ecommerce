using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Infrastructure.Common.Extensions;

public class ManagerOverrideToken<T>
{
  public string Permission { get; init; }
  public T Scope { get; init; }
}

public class ManagerOverrideToken : ManagerOverrideToken<object>
{
}

public static class HttpContextExtensions
{
  public static ManagerOverrideToken GetManagerOverrideToken(this HttpContext context)
  {
    return new ManagerOverrideToken
    {
      Permission = "Permissions.Brands.Create",
      Scope = new
      {
        Name = "brand11"
      }
    };

    // return new ManagerOverrideToken
    // {
    //   Permission = "Permissions.Brands.Delete",
    //   Scope = "08da48b0-515f-478e-8681-e99a622f6985"
    // };
  }
}