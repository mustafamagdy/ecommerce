using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

// Using a specific result object for clarity, though it could be a simple string or a more complex summary.
public class CalculateDepreciationResult
{
    public int AssetsProcessed { get; set; }
    public int DepreciationEntriesCreated { get; set; }
    public decimal TotalDepreciationAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CalculateDepreciationForPeriodRequest : IRequest<CalculateDepreciationResult>
{
    [Required]
    public DateTime PeriodEndDate { get; set; }

    public Guid? FixedAssetId { get; set; } // Optional: If null, calculate for all eligible assets.
                                           // If provided, calculate only for this asset.
}
