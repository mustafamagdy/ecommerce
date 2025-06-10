using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class FixedAssetDto : IDto
{
    public Guid Id { get; set; }
    public string AssetNumber { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string? Description { get; set; }
    public Guid AssetCategoryId { get; set; }
    public string? AssetCategoryName { get; set; } // For display
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }
    public Guid DepreciationMethodId { get; set; }
    public string? DepreciationMethodName { get; set; } // For display
    public string? Location { get; set; }
    public string Status { get; set; } = default!; // Mapped from FixedAssetStatus enum
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
