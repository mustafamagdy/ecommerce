namespace FSH.WebApi.Application.Identity.Tokens;

public interface ISystemSupportService : IScopedService
{
  Task<TokenResponse> RemoteLoginAsAdminForTenant(string tenantId, string username, CancellationToken cancellationToken);
  Task<string> ResetRemoteUserPassword(string tenantId, string username, string? newPassword, CancellationToken cancellationToken);
}