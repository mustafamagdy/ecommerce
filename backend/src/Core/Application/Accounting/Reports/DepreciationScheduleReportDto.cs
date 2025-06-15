using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class DepreciationScheduleReportDto
{
    public DateTime PeriodStartDate { get; set; } // Echo filter
    public DateTime PeriodEndDate { get; set; }   // Echo filter
    public Guid? AssetCategoryId { get; set; }    // Echo filter
    public string? AssetCategoryName { get; set; } // Populated if AssetCategoryId filter is used
    public Guid? FixedAssetId { get; set; }       // Echo filter
    public string? FixedAssetName { get; set; }   // Populated if FixedAssetId filter is used

    public List<DepreciationScheduleReportLineDto> Lines { get; set; } = new();

    public int TotalAssetsCount => Lines.Count; // Count of assets for which depreciation was calculated in period
    public decimal GrandTotalDepreciationForPeriod { get; set; } // Sum of DepreciationAmountForPeriod for all lines
    public decimal GrandTotalPurchaseCost { get; set; }
    public decimal GrandTotalAccumulatedDepreciationAtPeriodEnd { get; set; }
    public decimal GrandTotalBookValueAtPeriodEnd { get; set; }


    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class DepreciationScheduleReportLineDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetNumber { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string AssetCategoryName { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public string DepreciationMethodName { get; set; } = default!;
    public int UsefulLifeYears { get; set; }

    // Balances at the START of the reporting period (request.PeriodStartDate)
    public decimal AccumulatedDepreciationAtPeriodStart { get; set; }
    public decimal BookValueAtPeriodStart { get; set; } // PurchaseCost - AccumulatedDepreciationAtPeriodStart

    // Depreciation specifically for the requested period (request.PeriodStartDate to request.PeriodEndDate)
    public decimal DepreciationAmountForPeriod { get; set; }

    // Balances at the END of the reporting period (request.PeriodEndDate)
    public decimal AccumulatedDepreciationAtPeriodEnd { get; set; } // AccumulatedDepreciationAtPeriodStart + DepreciationAmountForPeriod
    public decimal BookValueAtPeriodEnd { get; set; } // BookValueAtPeriodStart - DepreciationAmountForPeriod (or PurchaseCost - AccDepAtPeriodEnd)

    public FixedAssetListingLineDto() { } // Renaming constructor to avoid conflict if file names are same
    public DepreciationScheduleReportLineDto() { }
}
