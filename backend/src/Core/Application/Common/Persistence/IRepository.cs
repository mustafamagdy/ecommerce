using System.Linq.Expressions;

namespace FSH.WebApi.Application.Common.Persistence;

// The Repository for the Application Db
// I(Read)RepositoryBase<T> is from Ardalis.Specification

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>
  where T : class, IAggregateRoot
{
  Task<List<T>> AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T>
  where T : class, IAggregateRoot
{
  Task<decimal?> SumAsync(ISpecification<T> specification, Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default);
}

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IRepositoryWithEvents<T> : IRepositoryBase<T>
  where T : class, IAggregateRoot
{
}

public interface INonAggregateRepository<TEntity> : IRepositoryBase<TEntity>
  where TEntity : class
{
  Task<List<TEntity>> AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));
}

public interface IReadNonAggregateRepository<TEntity> : IReadRepositoryBase<TEntity>
  where TEntity : class
{
}