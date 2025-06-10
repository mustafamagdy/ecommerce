using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class AssetCategoryDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? DefaultDepreciationMethodId { get; set; }
    public string? DefaultDepreciationMethodName { get; set; } // For display
    public int? DefaultUsefulLifeYears { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    // public bool IsActive { get; set; } // AssetCategory domain entity does not have IsActive yet. Add if needed.
}
