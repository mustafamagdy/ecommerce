using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;

namespace FSH.WebApi.Host.Controllers.Multitenancy;

public sealed class SupportController : VersionNeutralApiController
{
  [HttpPost("remote-admin-login")]
  [MustHavePermission(FSHAction.RemoteLogin, FSHResource.Tenants)]
  [OpenApiOperation("Root admin can remote login to tenant db to provide support", "")]
  public Task<TokenResponse> RemoteLogin(RemoteAdminLoginRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("reset-other-user-password")]
  [MustHavePermission(FSHAction.ResetPassword, FSHResource.Tenants)]
  [OpenApiOperation("Root admin can reset other tenant's user password to provide support", "")]
  public Task<string> ResetOtherTenantUser(ResetRemoteUserPasswordRequest request)
  {
    return Mediator.Send(request);
  }
}