using FSH.WebApi.Application.Identity.Tokens;

namespace FSH.WebApi.Application.Identity.Users;

public class ResetRemoteUserPasswordRequest : IRequest<string>
{
  public ResetRemoteUserPasswordRequest(string tenantId, string userName, string? password = null)
  {
    TenantId = tenantId;
    UserName = userName;
    Password = password;
  }

  public string TenantId { get; set; }
  public string UserName { get; set; }
  public string? Password { get; set; }
}

public class ResetRemoteUserPasswordRequestHandler : IRequestHandler<ResetRemoteUserPasswordRequest, string>
{
  private readonly ISystemSupportService _systemSupportService;

  public ResetRemoteUserPasswordRequestHandler(ISystemSupportService systemSupportService)
  {
    _systemSupportService = systemSupportService;
  }

  public Task<string> Handle(ResetRemoteUserPasswordRequest request, CancellationToken cancellationToken)
    => _systemSupportService.ResetRemoteUserPassword(request.TenantId, request.UserName, request.Password, cancellationToken);
}