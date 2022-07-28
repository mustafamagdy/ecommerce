using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Domain.Common.Contracts;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class DomainEventDispatcher : SaveChangesInterceptor
{
  private readonly IEventPublisher _eventPublisher;

  public DomainEventDispatcher(IEventPublisher eventPublisher)
  {
    _eventPublisher = eventPublisher;
  }

  public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
  {
    return SavedChangesAsync(eventData, result).GetAwaiter().GetResult();
  }

  public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = new CancellationToken())
  {
    await DispatchDomainEventsAsync(eventData.Context.ChangeTracker);
    return result;
  }

  private async Task DispatchDomainEventsAsync(ChangeTracker changeTracker)
  {
    var entitiesWithEvents = changeTracker
      .Entries<IEntity>()
      .Select(e => e.Entity)
      .Where(e => e.DomainEvents.Count > 0)
      .ToArray();

    foreach (var entity in entitiesWithEvents)
    {
      var events = entity.DomainEvents.ToArray();
      entity.ClearDomainEvents();
      foreach (var domainEvent in events)
      {
        await _eventPublisher.PublishAsync(domainEvent).ConfigureAwait(false);
      }
    }
  }
}