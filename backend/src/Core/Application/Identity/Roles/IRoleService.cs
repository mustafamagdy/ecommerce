namespace FSH.WebApi.Application.Identity.Roles;

public interface IRoleService : ITransientService
{
  Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken);
  Task<int> GetCountAsync(CancellationToken cancellationToken);
  Task<bool> ExistsAsync(string roleName, string? excludeId);
  Task<RoleDto> GetByIdAsync(string id);
  Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken);
  Task<RoleDto> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request);
  Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
  Task<string> DeleteRole(string id);
}