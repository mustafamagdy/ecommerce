using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Inventory;

public class InventoryItem : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int Quantity { get; private set; }

    public InventoryItem(string name, string? description, int quantity)
    {
        Name = name;
        Description = description;
        Quantity = quantity;
    }

    public InventoryItem Update(string? name, string? description, int? quantity)
    {
        if (name is not null && !Name.Equals(name)) Name = name;
        if (description is not null && Description != description) Description = description;
        if (quantity.HasValue && Quantity != quantity.Value) Quantity = quantity.Value;
        return this;
    }

    public void AdjustQuantity(int change)
    {
        Quantity += change;
    }
}
