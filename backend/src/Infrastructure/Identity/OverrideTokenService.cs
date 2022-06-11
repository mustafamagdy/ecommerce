using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Auth;
using FSH.WebApi.Infrastructure.Auth.Jwt;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FSH.WebApi.Infrastructure.Identity;

internal class OverrideTokenService : IOverrideTokenService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IStringLocalizer _t;
  private readonly SecuritySettings _securitySettings;
  private readonly JwtSettings _jwtSettings;
  private readonly FSHTenantInfo? _currentTenant;
  private readonly ITenantService _tenantService;
  private readonly IUserService _userService;

  public OverrideTokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IStringLocalizer<OverrideTokenService> localizer,
    FSHTenantInfo? currentTenant,
    IOptions<SecuritySettings> securitySettings,
    ITenantService tenantService, IUserService userService)
  {
    _userManager = userManager;
    _t = localizer;
    _jwtSettings = jwtSettings.Value;
    _currentTenant = currentTenant;
    _tenantService = tenantService;
    _userService = userService;
    _securitySettings = securitySettings.Value;
  }

  public async Task<OverrideTokenResponse> GetOverrideTokenAsync(OverrideTokenRequest request, string ipAddress, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(_currentTenant?.Id)
        || await _userManager.FindByEmailAsync(request.Email.Trim().Normalize()) is not { } user
        || !await _userManager.CheckPasswordAsync(user, request.Password)
        || !await _userService.HasPermissionAsync(user.Id, request.Permission, cancellationToken))
    {
      throw new UnauthorizedException(_t["Authentication Failed."]);
    }

    if (!user.IsActive)
    {
      throw new UnauthorizedException(_t["User Not Active. Please contact the administrator."]);
    }

    if (_securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
    {
      throw new UnauthorizedException(_t["E-Mail not confirmed."]);
    }

    if (_currentTenant.Id != MultitenancyConstants.Root.Id)
    {
      if (!_currentTenant.IsActive)
      {
        throw new UnauthorizedException(_t["Tenant is not Active. Please contact the Application Administrator."]);
      }

      if (_securitySettings.RequireActiveTenantSubscription && !(await HasAValidSubscription(_currentTenant.Id)))
      {
        throw new UnauthorizedException(
          _t["Tenant has no valid subscription. Please contact the Application Administrator."]);
      }
    }

    var managerOverrideScope = new ManagerOverrideToken(request.Permission, JsonSerializer.Serialize(request.Scope));
    return await GenerateTokensAndUpdateUser(user, managerOverrideScope);
  }

  public ManagerOverrideToken ExtractManagerOverrideTokenValues(string motToken)
  {
    var userPrincipal = GetPrincipalFromToken(motToken);
    string? motPermission = userPrincipal.GetMotPermission();
    string? motScopeJson = userPrincipal.GetMotScope();

    if (motPermission is null || motScopeJson is null)
    {
      throw new UnauthorizedException(_t["Invalid MOT Token."]);
    }

    return new ManagerOverrideToken(motPermission, motScopeJson);
  }

  private async Task<bool> HasAValidSubscription(string tenantId)
  {
    return (await _tenantService.GetActiveSubscriptions(tenantId)).Any();
  }

  private async Task<OverrideTokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user, ManagerOverrideToken mot)
  {
    string token = GenerateJwt(user, mot);
    return new OverrideTokenResponse(token);
  }

  private string GenerateJwt(ApplicationUser user, ManagerOverrideToken mot) =>
    GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user, mot));

  private IEnumerable<Claim> GetClaims(ApplicationUser user, ManagerOverrideToken mot) =>
    new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, user.Id),
      new(ClaimTypes.Email, user.Email),
      new(FSHClaims.Fullname, $"{user.FirstName} {user.LastName}"),
      new(FSHClaims.Tenant, _currentTenant!.Id),
      new(FSHClaims.ImageUrl, user.ImageUrl ?? string.Empty),
      new(FSHClaims.MOT_Permission, mot.Permission),
      new(FSHClaims.MOT_Scope, mot.Scope),
    };

  private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
  {
    var token = new JwtSecurityToken(
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.OverrideTokenExpirationInMinutes),
      signingCredentials: signingCredentials);
    var tokenHandler = new JwtSecurityTokenHandler();
    return tokenHandler.WriteToken(token);
  }

  private ClaimsPrincipal GetPrincipalFromToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.OverrideTokenKey)),
      ValidateIssuer = false,
      ValidateAudience = false,
      RoleClaimType = ClaimTypes.Role,
      ClockSkew = TimeSpan.Zero,
      ValidateLifetime = false
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

    if (securityToken is not JwtSecurityToken jwtSecurityToken
        || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
    {
      throw new UnauthorizedException(_t["Invalid Token."]);
    }

    return principal;
  }

  private SigningCredentials GetSigningCredentials()
  {
    byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.OverrideTokenKey);
    return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
  }
}