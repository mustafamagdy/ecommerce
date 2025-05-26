using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FSH.WebApi.Shared.Persistence;

public interface IUnitOfWork
{
  DbSet<T> Set<T>()
    where T : class;

  Task<int> CommitAsync(CancellationToken cancellationToken = default);

  string DebugLongView { get; }
  string DebugShortView { get; }

  ChangeTracker ChangeTracker { get; }
}

public interface IApplicationUnitOfWork : IUnitOfWork
{
}

public interface ITenantUnitOfWork : IUnitOfWork
{
}