using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class DepreciationScheduleReportRequest : IRequest<DepreciationScheduleReportDto>
{
    [ValidateNotEmpty] // Assuming custom attribute or FluentValidation rule
    public DateTime PeriodStartDate { get; set; }

    [ValidateNotEmpty] // Assuming custom attribute or FluentValidation rule
    public DateTime PeriodEndDate { get; set; }

    public Guid? AssetCategoryId { get; set; } // Optional filter
    public Guid? FixedAssetId { get; set; }    // Optional: for a single asset's schedule

    // Future enhancements:
    // public bool ProjectFuturePeriods { get; set; } = false; // To project depreciation beyond current entries
    // public int NumberOfFuturePeriodsToProject { get; set; } = 12; // e.g., 12 months

    // Validate that EndDate is not before StartDate (can be done via FluentValidation)
}
