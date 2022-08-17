using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Persistence;
using Mapster;

namespace FSH.WebApi.Infrastructure.Persistence.Repository;

public class NonAggregateDbRepository<TEntity> : RepositoryBase<TEntity>,
  IReadNonAggregateRepository<TEntity>, INonAggregateRepository<TEntity>
  where TEntity : class
{
  private readonly TenantUnitOfWork _uow;

  public NonAggregateDbRepository(TenantUnitOfWork uow)
    : base(uow)
  {
    _uow = uow;
  }

  protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification) =>
    specification.Selector is not null
      ? base.ApplySpecification(specification)
      : ApplySpecification(specification, false)
        .ProjectToType<TResult>();

  public async Task<List<TEntity>> AddRangeAsync(List<TEntity> entities,
    CancellationToken cancellationToken = default(CancellationToken))
  {
    await _uow.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    await SaveChangesAsync(cancellationToken);

    return entities;
  }
}