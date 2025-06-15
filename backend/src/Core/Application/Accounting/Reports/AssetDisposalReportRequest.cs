using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class AssetDisposalReportRequest : IRequest<AssetDisposalReportDto>
{
    public DateTime? StartDate { get; set; } // Filter by FixedAsset.DisposalDate
    public DateTime? EndDate { get; set; }   // Filter by FixedAsset.DisposalDate
    public Guid? AssetCategoryId { get; set; }
    // public string? DisposalReasonKeyword { get; set; } // Optional future filter
}
