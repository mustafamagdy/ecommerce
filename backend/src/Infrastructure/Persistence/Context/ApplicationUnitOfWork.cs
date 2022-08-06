using FSH.WebApi.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class ApplicationUnitOfWork : IApplicationUnitOfWork
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
}