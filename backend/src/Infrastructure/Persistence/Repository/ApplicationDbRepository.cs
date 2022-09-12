using System.Linq.Expressions;
using Ardalis.Specification;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Infrastructure.Persistence.Repository;

public sealed class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
  where T : class, IAggregateRoot
{
  private readonly ApplicationUnitOfWork _uow;

  public ApplicationDbRepository(ApplicationUnitOfWork uow)
    : base(uow)
  {
    _uow = uow;
  }

  // We override the default behavior when mapping to a dto.
  // We're using Mapster's ProjectToType here to immediately map the result from the database.
  // This is only done when no Selector is defined, so regular specifications with a selector also still work.
  protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification) =>
    specification.Selector is not null
      ? base.ApplySpecification(specification)
      : ApplySpecification(specification, false)
        .ProjectToType<TResult>();

  public async Task<List<T>> AddRangeAsync(List<T> entities,
    CancellationToken cancellationToken = default(CancellationToken))
  {
    await _uow.Set<T>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    await SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    return entities;
  }

  public Task<decimal?> SumAsync(ISpecification<T> specification, Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default)
  {
    var source = ApplySpecification(specification);
    return source.SumAsync(selector, cancellationToken: cancellationToken);
  }
}