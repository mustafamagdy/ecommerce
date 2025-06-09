using FSH.WebApi.Domain.Inventory;

namespace FSH.WebApi.Application.Inventory.Items;

public class SearchInventoryItemsRequest : PaginationFilter, IRequest<PaginationResponse<InventoryItemDto>>
{
    public string? Name { get; set; }
}

public class SearchInventoryItemsRequestHandler : IRequestHandler<SearchInventoryItemsRequest, PaginationResponse<InventoryItemDto>>
{
    private readonly IReadRepository<InventoryItem> _repository;

    public SearchInventoryItemsRequestHandler(IReadRepository<InventoryItem> repository) => _repository = repository;

    public async Task<PaginationResponse<InventoryItemDto>> Handle(SearchInventoryItemsRequest request, CancellationToken cancellationToken) =>
        await _repository.PaginatedListAsync(
            new InventoryItemsBySearchSpec(request),
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken);
}

public class InventoryItemsBySearchSpec : EntitiesByPaginationFilterSpec<InventoryItem, InventoryItemDto>
{
    public InventoryItemsBySearchSpec(SearchInventoryItemsRequest request)
        : base(request) =>
        Query
            .OrderBy(i => i.Name, !request.HasOrderBy())
            .Where(i => i.Name.Contains(request.Name!), !string.IsNullOrWhiteSpace(request.Name));
}
