using System.Security.Claims;
using FSH.WebApi.Application.Identity.Users.Password;

namespace FSH.WebApi.Application.Identity.Users;

public interface IUserService : ITransientService
{
  Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken);

  Task<bool> ExistsWithNameAsync(string name);
  Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
  Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);

  Task<PaginationResponse<UserDetailsDto>> GetListAsync(PaginationFilter filter, CancellationToken cancellationToken);
  Task<List<BasicUserDataDto>> GetListBasicDataAsync(CancellationToken cancellationToken);

  Task<int> GetCountAsync(CancellationToken cancellationToken);

  Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken);

  Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken);
  Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken);

  Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);
  Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
  Task InvalidatePermissionCacheAsync(string userId, CancellationToken cancellationToken);

  Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken);

  Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal);
  Task<UserDetailsDto> CreateAsync(CreateUserRequest request, string origin);
  Task UpdateAsync(UpdateUserRequest request, string userId);

  Task<string> ConfirmEmailAsync(string userId, string code, string tenant, CancellationToken cancellationToken);
  Task<string> ConfirmPhoneNumberAsync(string userId, string code);

  Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
  Task<string> ResetPasswordAsync(ResetPasswordRequest request);
  Task<string> ResetUserPasswordAsync(UserResetPasswordRequest request);
  Task ChangePasswordAsync(ChangePasswordRequest request, string userId);
}