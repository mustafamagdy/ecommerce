using MediatR;
using System;
using FSH.WebApi.Domain.Accounting; // For FixedAssetStatus enum if used directly

namespace FSH.WebApi.Application.Accounting.Reports;

public class FixedAssetListingRequest : IRequest<FixedAssetListingDto>
{
    public Guid? AssetCategoryId { get; set; }
    public string? Status { get; set; } // String to allow flexibility, parsed to FixedAssetStatus enum in handler
    public string? LocationKeyword { get; set; }

    // AsOfDate is important for calculating AccumulatedDepreciation and BookValue accurately
    // for a specific point in time. If not provided, "current" values will be assumed by the handler.
    public DateTime? AsOfDate { get; set; } // Default to null, handler can use DateTime.UtcNow if not set.
}
