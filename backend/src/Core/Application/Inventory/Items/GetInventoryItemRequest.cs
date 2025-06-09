using FSH.WebApi.Domain.Inventory;

namespace FSH.WebApi.Application.Inventory.Items;

public class GetInventoryItemRequest : IRequest<InventoryItemDto>
{
    public Guid Id { get; set; }

    public GetInventoryItemRequest(Guid id) => Id = id;
}

public class GetInventoryItemRequestHandler : IRequestHandler<GetInventoryItemRequest, InventoryItemDto>
{
    private readonly IRepository<InventoryItem> _repository;
    private readonly IStringLocalizer _t;

    public GetInventoryItemRequestHandler(IRepository<InventoryItem> repository, IStringLocalizer<GetInventoryItemRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<InventoryItemDto> Handle(GetInventoryItemRequest request, CancellationToken cancellationToken) =>
        await _repository.GetByIdAsync<InventoryItemDto>(request.Id, cancellationToken)
        ?? throw new NotFoundException(_t["Inventory Item {0} Not Found.", request.Id]);
}
