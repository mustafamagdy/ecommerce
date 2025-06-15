using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class AssetDisposalReportDto
{
    public DateTime? StartDate { get; set; } // Echo filter
    public DateTime? EndDate { get; set; }   // Echo filter
    public Guid? AssetCategoryId { get; set; } // Echo filter
    public string? AssetCategoryName { get; set; } // Populated if AssetCategoryId filter is used

    public List<AssetDisposalReportLineDto> Disposals { get; set; } = new();

    public int TotalDisposalsCount => Disposals.Count;
    public decimal GrandTotalOriginalCost { get; set; }
    public decimal GrandTotalAccumulatedDepreciationAtDisposal { get; set; }
    public decimal GrandTotalDisposalAmount { get; set; } // Sum of proceeds from disposal
    public decimal GrandTotalGainLossOnDisposal { get; set; }

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class AssetDisposalReportLineDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetNumber { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string AssetCategoryName { get; set; } = default!; // From FixedAsset.AssetCategory.Name
    public DateTime PurchaseDate { get; set; }
    public decimal OriginalCost { get; set; } // FixedAsset.PurchaseCost

    public DateTime DisposalDate { get; set; } // From FixedAsset.DisposalDate (should be non-null for this report)
    public string? DisposalReason { get; set; } // From FixedAsset.DisposalReason
    public decimal DisposalAmount { get; set; } // Proceeds from disposal (from FixedAsset.DisposalAmount, defaults to 0 if null)

    public decimal AccumulatedDepreciationAtDisposal { get; set; } // Calculated up to DisposalDate
    public decimal BookValueAtDisposal { get; set; } // OriginalCost - AccumulatedDepreciationAtDisposal
    public decimal GainLossOnDisposal { get; set; } // DisposalAmount - BookValueAtDisposal

    public AssetDisposalReportLineDto() { }
}
