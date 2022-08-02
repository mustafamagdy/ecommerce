using System.Reflection;
using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Domain.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class FixNpgDateTimeKind : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    return SavingChangesAsync(eventData, result).GetAwaiter().GetResult();
  }

  public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new CancellationToken())
  {
    await FixDateTimeKind(eventData.Context.ChangeTracker);
    return result;
  }

  private static void SetPrivateFieldValue<T>(object obj, string propName, T val)
  {
    if (obj == null) throw new ArgumentNullException(nameof(obj));
    Type t = obj.GetType();
    FieldInfo fi = null;
    while (fi == null && t != null)
    {
      fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (fi == null)
      {
        var backingField = $"<{propName}>k__BackingField";
        fi = t.GetField(backingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      }

      t = t.BaseType;
    }

    if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
    fi.SetValue(obj, val);
  }

  private async Task FixDateTimeKind(ChangeTracker changeTracker)
  {
    var dateProperties = changeTracker.Context.Model.GetEntityTypes()
      .SelectMany(t => t.GetProperties())
      .Where(p => p.ClrType == typeof(DateTime))
      .Select(z => new
      {
        ParentName = z.DeclaringEntityType.Name,
        PropertyName = z.Name
      });

    var editedEntitiesInTheDbContextGraph = changeTracker.Entries()
      .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
      .Select(x => x.Entity);

    foreach (var entity in editedEntitiesInTheDbContextGraph)
    {
      var entityFields = dateProperties.Where(d => d.ParentName == entity.GetType().FullName);

      foreach (var property in entityFields)
      {
        var prop = entity.GetType().GetProperty(property.PropertyName);

        if (prop == null)
          continue;

        var originalValue = prop.GetValue(entity) as DateTime?;
        if (originalValue == null)
          continue;

        if (prop.SetMethod != null)
        {
          prop.SetValue(entity, DateTime.SpecifyKind(originalValue.Value, DateTimeKind.Utc));
        }
        else
        {
          SetPrivateFieldValue(entity, prop.Name, DateTime.SpecifyKind(originalValue.Value, DateTimeKind.Utc));
        }
      }
    }
  }
}

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