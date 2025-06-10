using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class AssetCategory : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? DefaultDepreciationMethodId { get; private set; } // Nullable Guid
    // public virtual DepreciationMethod? DefaultDepreciationMethod { get; private set; } // Navigation
    public int? DefaultUsefulLifeYears { get; private set; } // Optional
    public bool IsActive { get; private set; } = true; // Default to true

    // Private constructor for EF Core
    private AssetCategory() { }

    public AssetCategory(
        string name,
        string? description,
        Guid? defaultDepreciationMethodId,
        int? defaultUsefulLifeYears,
        bool isActive = true)
    {
        Name = name;
        Description = description;
        DefaultDepreciationMethodId = defaultDepreciationMethodId;
        DefaultUsefulLifeYears = defaultUsefulLifeYears;
        IsActive = isActive;

        if (defaultUsefulLifeYears.HasValue && defaultUsefulLifeYears.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(defaultUsefulLifeYears), "Default useful life must be positive if specified.");
    }

    public AssetCategory Update(
        string? name,
        string? description,
        Guid? defaultDepreciationMethodId, // Allow unsetting by passing null
        int? defaultUsefulLifeYears,    // Allow unsetting by passing null
        bool? isActive)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (description is not null && Description?.Equals(description) is not true) Description = description;

        // Handle setting/unsetting DefaultDepreciationMethodId
        DefaultDepreciationMethodId = defaultDepreciationMethodId;

        if (defaultUsefulLifeYears.HasValue && defaultUsefulLifeYears.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(defaultUsefulLifeYears), "Default useful life must be positive if specified.");
        DefaultUsefulLifeYears = defaultUsefulLifeYears;
        if (isActive.HasValue && IsActive != isActive.Value) IsActive = isActive.Value;

        return this;
    }
}
