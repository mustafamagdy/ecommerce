using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
  Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);
  Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
  Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress, Guid? branchId = null);
}

public interface IOverrideTokenService : ITransientService
{
  Task<OverrideTokenResponse> GetOverrideTokenAsync(OverrideTokenRequest request, string ipAddress, CancellationToken cancellationToken);
  ManagerOverrideToken ExtractManagerOverrideTokenValues(string motToken);
}