using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Categories.EventHandlers;

public class CategoryCreatedEventHandler : EventNotificationHandler<EntityCreatedEvent<Category>>
{
    private readonly ILogger<CategoryCreatedEventHandler> _logger;

    public CategoryCreatedEventHandler(ILogger<CategoryCreatedEventHandler> logger) => _logger = logger;

    public override Task Handle(EntityCreatedEvent<Category> @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered", @event.GetType().Name);
        return Task.CompletedTask;
    }
}