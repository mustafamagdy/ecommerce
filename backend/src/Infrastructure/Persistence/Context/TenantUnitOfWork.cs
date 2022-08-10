using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class TenantUnitOfWork : ITenantUnitOfWork
{
  private readonly TenantDbContext _dbContext;

  public TenantUnitOfWork(TenantDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public DbSet<T> Set<T>()
    where T : class => _dbContext.Set<T>();

  public Task<int> CommitAsync(CancellationToken cancellationToken)
  {
    return _dbContext.SaveChangesAsync(cancellationToken);
  }

  public string DebugLongView => _dbContext.ChangeTracker.DebugView.LongView;
  public string DebugShortView => _dbContext.ChangeTracker.DebugView.ShortView;
  public ChangeTracker ChangeTracker => _dbContext.ChangeTracker;
}