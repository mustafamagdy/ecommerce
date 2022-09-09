namespace FSH.WebApi.Application.Common.Specification;

public class EntitiesByIdSpec<T, TKey> : Specification<T>
  where T : Domain.Common.Contracts.IEntity<TKey>
{
  public EntitiesByIdSpec(TKey id) =>
    Query.Where(e => Equals(e.Id, id));
}