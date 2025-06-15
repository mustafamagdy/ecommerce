using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class FixedAssetListingDto
{
    public Guid? AssetCategoryId { get; set; } // Echo filter
    public string? AssetCategoryName { get; set; } // Populated if AssetCategoryId filter is used
    public string? StatusFilter { get; set; }      // Echo filter
    public string? LocationFilter { get; set; }    // Echo filter
    public DateTime? AsOfDate { get; set; }         // Echo filter, or the date used for calculations

    public List<FixedAssetListingLineDto> Assets { get; set; } = new();

    public int TotalCount => Assets.Count;
    public decimal GrandTotalPurchaseCost { get; set; }
    public decimal GrandTotalAccumulatedDepreciation { get; set; }
    public decimal GrandTotalBookValue { get; set; }

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class FixedAssetListingLineDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetNumber { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string? Description { get; set; } // Added Description from FixedAsset entity

    public Guid AssetCategoryId { get; set; }
    public string AssetCategoryName { get; set; } = default!;

    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }

    public Guid DepreciationMethodId { get; set; }
    public string DepreciationMethodName { get; set; } = default!;

    // These are point-in-time values, calculated as of AsOfDate
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; } // PurchaseCost - AccumulatedDepreciation

    public string Status { get; set; } = default!; // From FixedAsset.Status enum, converted to string
    public string? Location { get; set; }
    public DateTime? DisposalDate { get; set; } // From FixedAsset.DisposalDate
    // public string? DisposalReason { get; set; } // Optional extra detail
    // public decimal? DisposalAmount { get; set; } // Optional extra detail

    public FixedAssetListingLineDto() { }
}
