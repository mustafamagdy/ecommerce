using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Infrastructure.Identity;

public class SystemSupportService : ISystemSupportService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;

  public SystemSupportService(IServiceProvider serviceProvider, IReadNonAggregateRepository<FSHTenantInfo> repo)
  {
    _serviceProvider = serviceProvider;
    _repo = repo;
  }

  public async Task<TokenResponse> RemoteLoginAsAdminForTenant(string tenantId)
  {
    var tenant = await _repo.GetByIdAsync(tenantId);

    await using var scope = _serviceProvider.CreateAsyncScope();
    var sp = scope.ServiceProvider;
    sp.GetRequiredService<IMultiTenantContextAccessor>()
      .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
    {
      TenantInfo = tenant
    };
    var db = sp.GetRequiredService<ApplicationDbContext>();
    var users = db.Users.ToList();
    var admin = users.First();
    var tokenService = sp.GetRequiredService<ITokenService>();

    return await tokenService.GenerateTokensAndUpdateUser(admin, "ROOT_ADMIN_SUPPORT");
  }
}