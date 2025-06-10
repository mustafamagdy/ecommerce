using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class DepreciationMethod : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!; // E.g., "Straight-Line", "Double Declining Balance"
    public string? Description { get; private set; }
    // Could add formula parameters or other relevant properties in a more advanced version

    // Private constructor for EF Core
    private DepreciationMethod() { }

    public DepreciationMethod(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public DepreciationMethod Update(string? name, string? description)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        return this;
    }
}
