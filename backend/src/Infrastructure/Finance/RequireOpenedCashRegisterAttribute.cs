using FSH.WebApi.Infrastructure.OpenApi;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Infrastructure.Finance;

public sealed class RequireOpenedCashRegisterAttribute : SwaggerHeaderAttribute
{
  public RequireOpenedCashRegisterAttribute()
    : base(MultitenancyConstants.CashRegisterHeaderName, "Cash register Id", string.Empty, true)
  {
  }
}