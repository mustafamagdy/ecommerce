using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Infrastructure.OpenApi;

public sealed class TenantIdHeaderAttribute : SwaggerHeaderAttribute
{
  public TenantIdHeaderAttribute()
    : base(MultitenancyConstants.TenantIdName, "Input your tenant Id to access this API", string.Empty, true)
  {
  }
}