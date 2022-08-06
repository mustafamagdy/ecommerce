using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Shared.Persistence;

public interface IUnitOfWork
{
  DbSet<T> Set<T>() where T : class;
  Task<int> CommitAsync(CancellationToken cancellationToken = default);
}

public interface IApplicationUnitOfWork : IUnitOfWork
{
}

public interface ITenantUnitOfWork : IUnitOfWork
{
}