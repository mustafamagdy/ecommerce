namespace FSH.WebApi.Application.Inventory.Items;

public class InventoryItemDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Quantity { get; set; }
}
