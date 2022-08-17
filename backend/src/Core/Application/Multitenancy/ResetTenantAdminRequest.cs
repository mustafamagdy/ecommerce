using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Identity.Users.Password;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class ResetTenantAdminRequest : IRequest<string>
{
  public string TenantId { get; set; }
  public string? Password { get; set; }

  public ResetTenantAdminRequest(string tenantId, string? password)
  {
    TenantId = tenantId;
    Password = password;
  }
}

public class ResetTenantAdminRequestValidator : CustomValidator<ResetTenantAdminRequest>
{
  public ResetTenantAdminRequestValidator() =>
    RuleFor(t => t.TenantId).NotEmpty();
}

public class ResetTenantAdminRequestHandler : IRequestHandler<ResetTenantAdminRequest, string>
{
  private readonly ITenantService _tenantService;
  private readonly IUserService _userService;
  private readonly IStringLocalizer<ResetTenantAdminRequestHandler> _t;

  public ResetTenantAdminRequestHandler(ITenantService tenantService, IStringLocalizer<ResetTenantAdminRequestHandler> localizer, IUserService userService)
  {
    _tenantService = tenantService;
    _t = localizer;
    _userService = userService;
  }

  public async Task<string> Handle(ResetTenantAdminRequest request, CancellationToken cancellationToken)
  {
    // var tenant = await _tenantService.GetByIdAsync(request.TenantId);
    // _ = tenant ?? throw new NotFoundException(_t["Tenant {0} not found "]);
    // if (tenant.Id == MultitenancyConstants.RootTenant.Id)
    // {
    //   throw new InvalidOperationException(_t["Cannot reset admin account for root tenant"]);
    // }
    //
    // var adminUserId = await _tenantService.GetAdminUserIdAsync(request.TenantId);
    // var newPassword = await _userService.ResetUserPasswordAsync(new UserResetPasswordRequest(adminUserId, request.Password));
    // //todo
    // return newPassword;

    return "";
  }
}