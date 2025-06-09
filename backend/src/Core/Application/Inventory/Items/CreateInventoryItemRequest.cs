using FSH.WebApi.Domain.Inventory;
using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Inventory.Items;

public class CreateInventoryItemRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Quantity { get; set; }
}

public class CreateInventoryItemRequestValidator : CustomValidator<CreateInventoryItemRequest>
{
    public CreateInventoryItemRequestValidator()
    {
        RuleFor(i => i.Name).NotEmpty().MaximumLength(75);
    }
}

public class CreateInventoryItemRequestHandler : IRequestHandler<CreateInventoryItemRequest, Guid>
{
    private readonly IRepository<InventoryItem> _repository;

    public CreateInventoryItemRequestHandler(IRepository<InventoryItem> repository) => _repository = repository;

    public async Task<Guid> Handle(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem(request.Name, request.Description, request.Quantity);
        item.AddDomainEvent(EntityCreatedEvent.WithEntity(item));

        await _repository.AddAsync(item, cancellationToken);

        return item.Id;
    }
}
