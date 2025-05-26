using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class EFCoreCustomStore<TEFCoreStoreDbContext, TTenantInfo>
  : EFCoreStore<TEFCoreStoreDbContext, TTenantInfo>
  where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
  where TTenantInfo : class, ITenantInfo, new()
{
  private readonly TEFCoreStoreDbContext _dbContext;

  public EFCoreCustomStore(TEFCoreStoreDbContext dbContext)
    : base(dbContext)
  {
    _dbContext = dbContext;
  }

  public override async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
  {
    await _dbContext.TenantInfo.AddAsync(tenantInfo);
    return true;
  }

  public override async Task<bool> TryRemoveAsync(string identifier)
  {
    var existing = await _dbContext.TenantInfo
      .Where(ti => ti.Identifier == identifier)
      .SingleOrDefaultAsync();

    if (existing is null)
    {
      return false;
    }

    _dbContext.TenantInfo.Remove(existing);
    return true;
  }

  public override Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
  {
    _dbContext.TenantInfo.Update(tenantInfo);
    return Task.FromResult(true);
  }
}