using FSH.WebApi.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public sealed class ApplicationUnitOfWork : IApplicationUnitOfWork
{
  private readonly ApplicationDbContext _dbContext;

  public ApplicationUnitOfWork(ApplicationDbContext dbContext)
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