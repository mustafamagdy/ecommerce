namespace FSH.WebApi.Domain.Common.Contracts;

public interface IEntity
{
  IReadOnlyList<DomainEvent> DomainEvents { get; }

  void AddDomainEvent(DomainEvent @event);
  void ClearDomainEvents();
}

public interface IEntity<out TId> : IEntity
{
  TId Id { get; }
}