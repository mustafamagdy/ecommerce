using FSH.WebApi.Application.Identity.Tokens;

namespace FSH.WebApi.Host.Controllers.Identity;

public sealed class TokensController : VersionNeutralApiController
{
  private readonly ITokenService _tokenService;
  private readonly IOverrideTokenService _overrideTokenService;

  public TokensController(ITokenService tokenService, IOverrideTokenService overrideTokenService)
  {
    _tokenService = tokenService;
    _overrideTokenService = overrideTokenService;
  }

  [HttpPost]
  [AllowAnonymous]
  [TenantIdHeader]
  [OpenApiOperation("Request an access token using credentials.", "")]
  public Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
  {
    return _tokenService.GetTokenAsync(request, GetIpAddress(), cancellationToken);
  }

  [HttpPost("mot")]
  [AllowAnonymous]
  [TenantIdHeader]
  [OpenApiOperation("Request a manager override token using credentials, that can be used to bypass a specific operation for a user.", "")]
  public Task<OverrideTokenResponse> GetManagerOverrideTokenAsync(OverrideTokenRequest request, CancellationToken cancellationToken)
  {
    return _overrideTokenService.GetOverrideTokenAsync(request, GetIpAddress(), cancellationToken);
  }

  [HttpPost("refresh")]
  [AllowAnonymous]
  [TenantIdHeader]
  [OpenApiOperation("Request an access token using a refresh token.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
  {
    return _tokenService.RefreshTokenAsync(request, GetIpAddress());
  }

  private string GetIpAddress() =>
    Request.Headers.ContainsKey("X-Forwarded-For")
      ? Request.Headers["X-Forwarded-For"]
      : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
}