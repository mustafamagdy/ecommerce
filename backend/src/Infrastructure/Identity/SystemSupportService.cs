using System.Collections.Immutable;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Identity.Users.Password;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Multitenancy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Infrastructure.Identity;

public sealed class SystemSupportService : ISystemSupportService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;

  public SystemSupportService(IServiceProvider serviceProvider, IReadNonAggregateRepository<FSHTenantInfo> repo)
  {
    _serviceProvider = serviceProvider;
    _repo = repo;
  }

  public async Task<TokenResponse> RemoteLoginAsAdminForTenant(string tenantId, string username, SubscriptionType subscription, CancellationToken cancellationToken)
  {
    if (tenantId == MultitenancyConstants.RootTenant.Id)
    {
      throw new InvalidOperationException("Remote login to root tenant is not valid operation");
    }

    var tenant = await _repo.GetByIdAsync(tenantId, cancellationToken);

    await using var scope = _serviceProvider.CreateAsyncScope();
    var sp = scope.ServiceProvider;
    sp.GetRequiredService<IMultiTenantContextAccessor>()
      .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
    {
      TenantInfo = tenant
    };
    var db = sp.GetRequiredService<ApplicationDbContext>();
    var admin = await db.Users.FirstOrDefaultAsync(a => a.UserName.ToLower() == username, cancellationToken: cancellationToken)
                ?? throw new NotFoundException($"User {username} not found");
    var tokenService = sp.GetRequiredService<ITokenService>();

    return await tokenService.GenerateTokensAndUpdateUser(admin, "ROOT_ADMIN_REMOTE_SUPPORT");
  }

  public async Task<string> ResetRemoteUserPassword(string tenantId, string username, string? newPassword,
    SubscriptionType subscription, CancellationToken cancellationToken)
  {
    if (tenantId == MultitenancyConstants.RootTenant.Id)
    {
      throw new InvalidOperationException("Remote login to root tenant is not valid operation");
    }

    var tenant = await _repo.GetByIdAsync(tenantId, cancellationToken);

    await using var scope = _serviceProvider.CreateAsyncScope();
    var sp = scope.ServiceProvider;
    sp.GetRequiredService<IMultiTenantContextAccessor>()
      .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
    {
      TenantInfo = tenant
    };
    var db = sp.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.FirstOrDefaultAsync(a => a.UserName.ToLower() == username.ToLower(), cancellationToken: cancellationToken)
               ?? throw new NotFoundException($"User {username} not found");
    var userService = sp.GetRequiredService<IUserService>();
    return await userService.ResetUserPasswordAsync(new UserResetPasswordRequest(Guid.Parse(user.Id), newPassword));
  }
}