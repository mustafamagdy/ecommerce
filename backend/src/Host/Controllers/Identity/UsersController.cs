using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Identity.Users.Password;

namespace FSH.WebApi.Host.Controllers.Identity;

public sealed class UsersController : VersionNeutralApiController
{
  private readonly IUserService _userService;

  public UsersController(IUserService userService) => _userService = userService;

  [HttpPost("search")]
  [MustHavePermission(FSHAction.View, FSHResource.Users)]
  [OpenApiOperation("Get list of all users.", "")]
  public Task<PaginationResponse<UserDetailsDto>> GetListAsync(PaginationFilter filter, CancellationToken cancellationToken)
  {
    return _userService.GetListAsync(filter,cancellationToken);
  }

  [HttpGet("basic")]
  [MustHavePermission(FSHAction.View, FSHResource.Users)]
  [OpenApiOperation("Get basic list of all users.", "")]
  public Task<List<BasicUserDataDto>> GetBasicListAsync(CancellationToken cancellationToken)
  {
    return _userService.GetListBasicDataAsync(cancellationToken);
  }

  [HttpGet("{id}")]
  [MustHavePermission(FSHAction.View, FSHResource.Users)]
  [OpenApiOperation("Get a user's details.", "")]
  public Task<UserDetailsDto> GetByIdAsync(string id, CancellationToken cancellationToken)
  {
    return _userService.GetAsync(id, cancellationToken);
  }

  [HttpGet("{id}/roles")]
  [MustHavePermission(FSHAction.View, FSHResource.UserRoles)]
  [OpenApiOperation("Get a user's roles.", "")]
  public Task<List<UserRoleDto>> GetRolesAsync(string id, CancellationToken cancellationToken)
  {
    return _userService.GetRolesAsync(id, cancellationToken);
  }

  [HttpPost("{id}/roles")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  [MustHavePermission(FSHAction.Update, FSHResource.UserRoles)]
  [OpenApiOperation("Update a user's assigned roles.", "")]
  public Task<string> AssignRolesAsync(string id, UserRolesRequest request, CancellationToken cancellationToken)
  {
    return _userService.AssignRolesAsync(id, request, cancellationToken);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Users)]
  [OpenApiOperation("Creates a new user.", "")]
  public Task<UserDetailsDto> CreateAsync(CreateUserRequest request)
  {
    // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
    // and return UnAuthorized when it isn't
    // Also: add other protection to prevent automatic posting (captcha?)
    return _userService.CreateAsync(request, GetOriginFromRequest());
  }

  [HttpPost("{id}/reset-password")]
  [OpenApiOperation("Reset a user's password.", "")]
  [MustHavePermission(FSHAction.ResetPassword, FSHResource.Users)]
  public Task<string> ResetPasswordForUserAsync(string id, UserResetPasswordRequest request)
  {
    return _userService.ResetUserPasswordAsync(request);
  }

  [HttpPost("self-register")]
  [TenantIdHeader]
  [AllowAnonymous]
  [OpenApiOperation("Anonymous user creates a user.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  public Task<UserDetailsDto> SelfRegisterAsync(CreateUserRequest request)
  {
    // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
    // and return UnAuthorized when it isn't
    // Also: add other protection to prevent automatic posting (captcha?)
    return _userService.CreateAsync(request, GetOriginFromRequest());
  }

  [HttpPost("{id}/toggle-status")]
  [MustHavePermission(FSHAction.Update, FSHResource.Users)]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  [OpenApiOperation("Toggle a user's active status.", "")]
  public async Task<ActionResult> ToggleStatusAsync(string id, ToggleUserStatusRequest request,
    CancellationToken cancellationToken)
  {
    if (id != request.UserId)
    {
      return BadRequest();
    }

    await _userService.ToggleStatusAsync(request, cancellationToken);
    return Ok();
  }

  [HttpGet("confirm-email")]
  [AllowAnonymous]
  [OpenApiOperation("Confirm email address for a user.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<string> ConfirmEmailAsync([FromQuery] string tenant, [FromQuery] string userId, [FromQuery] string code,
    CancellationToken cancellationToken)
  {
    return _userService.ConfirmEmailAsync(userId, code, tenant, cancellationToken);
  }

  [HttpGet("confirm-phone-number")]
  [AllowAnonymous]
  [OpenApiOperation("Confirm phone number for a user.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<string> ConfirmPhoneNumberAsync([FromQuery] string userId, [FromQuery] string code)
  {
    return _userService.ConfirmPhoneNumberAsync(userId, code);
  }

  [HttpPost("forgot-password")]
  [AllowAnonymous]
  [TenantIdHeader]
  [OpenApiOperation("Request a password reset email for a user.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
  {
    return _userService.ForgotPasswordAsync(request, GetOriginFromRequest());
  }

  [HttpPost("reset-password")]
  [AllowAnonymous]
  [OpenApiOperation("Reset a user's password using reset password token.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
  {
    return _userService.ResetPasswordAsync(request);
  }

  private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}