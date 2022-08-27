using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Identity.Tokens;

public class RemoteAdminLoginRequest : IRequest<TokenResponse>
{
  public string TenantId { get; set; }
  public string UserName { get; set; }
  public SubscriptionType Subscription { get; set; }
}

public class RemoteAdminLoginRequestHandler : IRequestHandler<RemoteAdminLoginRequest, TokenResponse>
{
  private ISystemSupportService _systemSupportService;

  public RemoteAdminLoginRequestHandler(ISystemSupportService systemSupportService)
  {
    _systemSupportService = systemSupportService;
  }

  public Task<TokenResponse> Handle(RemoteAdminLoginRequest request, CancellationToken cancellationToken)
    => _systemSupportService.RemoteLoginAsAdminForTenant(request.TenantId, request.UserName, request.Subscription, cancellationToken);
}