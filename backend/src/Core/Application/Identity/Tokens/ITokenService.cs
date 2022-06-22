namespace FSH.WebApi.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
  Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);
  Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
}

public interface IOverrideTokenService : ITransientService
{
  Task<OverrideTokenResponse> GetOverrideTokenAsync(OverrideTokenRequest request, string ipAddress, CancellationToken cancellationToken);
  ManagerOverrideToken ExtractManagerOverrideTokenValues(string motToken);
}