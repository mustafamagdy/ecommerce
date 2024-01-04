using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Categories.EventHandlers;

public class CategoryDeletedEventHandler : EventNotificationHandler<EntityDeletedEvent<Category>>
{
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(ILogger<CategoryDeletedEventHandler> logger) => _logger = logger;

    public override Task Handle(EntityDeletedEvent<Category> @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{event} Triggered", @event.GetType().Name);
        return Task.CompletedTask;
    }
}