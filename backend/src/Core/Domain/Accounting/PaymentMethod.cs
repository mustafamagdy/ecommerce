using FSH.WebApi.Domain.Common.Contracts;

namespace FSH.WebApi.Domain.Accounting;

public class PaymentMethod : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public PaymentMethod(string name, string? description, bool isActive = true)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public PaymentMethod Update(string? name, string? description, bool? isActive)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        if (isActive.HasValue && IsActive != isActive.Value) IsActive = isActive.Value;
        return this;
    }
}
