using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Categories.EventHandlers;

public class CategoryUpdatedEventHandler : EventNotificationHandler<EntityUpdatedEvent<Category>>
{
  private readonly ILogger<CategoryUpdatedEventHandler> _logger;

  public CategoryUpdatedEventHandler(ILogger<CategoryUpdatedEventHandler> logger) => _logger = logger;

  public override Task Handle(EntityUpdatedEvent<Category> @event, CancellationToken cancellationToken)
  {
    _logger.LogInformation("{event} Triggered", @event.GetType().Name);
    return Task.CompletedTask;
  }
}