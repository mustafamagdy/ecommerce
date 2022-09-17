using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace FSH.WebApi.Domain.Common.Contracts;

public abstract class BaseEntity : BaseEntity<DefaultIdType>
{
  protected BaseEntity() => Id = NewId.Next().ToGuid();
}

public abstract class BaseEntity<TId> : IEntity<TId>
{
  private readonly List<DomainEvent> _domainEvents = new();
  public TId Id { get; set; } = default!;

  [NotMapped]
  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public void AddDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
  public void ClearDomainEvents() => _domainEvents.Clear();
}