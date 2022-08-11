namespace FSH.WebApi.Domain.Common.Contracts;

public interface IEntity
{
  IReadOnlyCollection<DomainEvent> DomainEvents { get; }

  void AddDomainEvent(DomainEvent @event);
  void ClearDomainEvents();
}

public interface IEntity<out TId> : IEntity
{
  TId Id { get; }
}