using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Identity.Tokens;

public interface ISystemSupportService : IScopedService
{
  Task<TokenResponse> RemoteLoginAsAdminForTenant(string tenantId, string username, SubscriptionType subscription, CancellationToken cancellationToken);
  Task<string> ResetRemoteUserPassword(string tenantId, string username, string? newPassword, SubscriptionType subscription, CancellationToken cancellationToken);
}