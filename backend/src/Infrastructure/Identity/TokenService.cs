using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ardalis.Specification;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Auth;
using FSH.WebApi.Infrastructure.Auth.Jwt;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FSH.WebApi.Infrastructure.Identity;

internal sealed class TokenService : ITokenService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IStringLocalizer _t;
  private readonly IdentitySettings _identitySettings;
  private readonly SecuritySettings _securitySettings;
  private readonly JwtSettings _jwtSettings;
  private readonly FSHTenantInfo? _currentTenant;
  private readonly ISystemTime _systemTime;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;

  public TokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IOptions<IdentitySettings> identitySettings,
    IStringLocalizer<TokenService> localizer,
    FSHTenantInfo? currentTenant,
    IOptions<SecuritySettings> securitySettings,
    ISystemTime systemTime, IReadNonAggregateRepository<FSHTenantInfo> repo)
  {
    _userManager = userManager;
    _t = localizer;
    _identitySettings = identitySettings.Value;
    _jwtSettings = jwtSettings.Value;
    _currentTenant = currentTenant;
    _systemTime = systemTime;
    _repo = repo;
    _securitySettings = securitySettings.Value;
  }

  public async Task<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(_currentTenant?.Id)
        || await _userManager.FindByEmailAsync(request.Email.Trim().Normalize()) is not { } user
        || !await _userManager.CheckPasswordAsync(user, request.Password))
    {
      throw new UnauthorizedException(_t["Authentication Failed."]);
    }

    if (!user.Active)
    {
      throw new UnauthorizedException(_t["User Not Active. Please contact the administrator."]);
    }

    if (_identitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
    {
      throw new UnauthorizedException(_t["E-Mail not confirmed."]);
    }

    if (_currentTenant.Id != MultitenancyConstants.Root.Id)
    {
      if (!_currentTenant.Active)
      {
        throw new UnauthorizedException(_t["Tenant is not Active. Please contact the Application Administrator."]);
      }

      if (_securitySettings.RequireActiveTenantSubscription && !(await HasAValidSubscription(_currentTenant.Id)))
      {
        throw new UnauthorizedException(
          _t["Tenant has no valid subscription. Please contact the Application Administrator."]);
      }
    }

    return await GenerateTokensAndUpdateUser(user, ipAddress, request.BranchId);
  }

  private Task<bool> HasAValidSubscription(string tenantId)
  {
    var today = _systemTime.Now;

    var spec = new SingleResultSpecification<FSHTenantInfo>()
      .Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History)
      .Where(a => a.Id == tenantId
                  && a.Active
                  && a.ProdSubscription != null
                  && a.ProdSubscription.ExpiryDate >= today);

    return _repo.AnyAsync(spec.Specification);
  }

  public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
  {
    var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
    string? userEmail = userPrincipal.GetEmail();
    var user = await _userManager.FindByEmailAsync(userEmail);
    if (user is null)
    {
      throw new UnauthorizedException(_t["Authentication Failed."]);
    }

    if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    {
      throw new UnauthorizedException(_t["Invalid Refresh Token."]);
    }

    return await GenerateTokensAndUpdateUser(user, ipAddress);
  }

  public async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, string ipAddress, Guid? branchId = null)
  {
    string token = GenerateJwt(user, ipAddress, branchId);

    user.RefreshToken = GenerateRefreshToken();
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
    if (branchId != null)
    {
      user.LastUsedBranchId = branchId;
    }

    await _userManager.UpdateAsync(user);

    return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
  }

  private string GenerateJwt(ApplicationUser user, string ipAddress, Guid? branchId) =>
    GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, ipAddress, branchId));

  private IEnumerable<Claim> GetClaims(ApplicationUser user, string ipAddress, Guid? branchId)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, user.Id),
      new(ClaimTypes.Email, user.Email),
      new(FSHClaims.Fullname, $"{user.FirstName} {user.LastName}"),
      new(ClaimTypes.Name, user.FirstName ?? string.Empty),
      new(ClaimTypes.Surname, user.LastName ?? string.Empty),
      new(FSHClaims.IpAddress, ipAddress),
      new(FSHClaims.Tenant, _currentTenant!.Id),
      new(FSHClaims.Branch, (string.IsNullOrEmpty(user.LastUsedBranchId.ToString()) ? branchId?.ToString() : user.LastUsedBranchId.ToString()) ?? string.Empty),
      new(FSHClaims.ImageUrl, user.ImagePath ?? string.Empty),
      new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
    };

    if (user.MustChangePassword)
    {
      claims.Add(new(FSHClaims.MustChangePassword, string.Empty));
    }

    return claims;
  }

  private string GenerateRefreshToken()
  {
    byte[] randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
  {
    var token = new JwtSecurityToken(
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
      signingCredentials: signingCredentials);
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.WriteToken(token);
  }

  private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
      ValidateIssuer = false,
      ValidateAudience = false,
      RoleClaimType = ClaimTypes.Role,
      ClockSkew = TimeSpan.Zero,
      ValidateLifetime = false
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
    if (securityToken is not JwtSecurityToken jwtSecurityToken ||
        !jwtSecurityToken.Header.Alg.Equals(
          SecurityAlgorithms.HmacSha256,
          StringComparison.InvariantCultureIgnoreCase))
    {
      throw new UnauthorizedException(_t["Invalid Token."]);
    }

    return principal;
  }

  private SigningCredentials GetSigningCredentials()
  {
    byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
    return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
  }
}