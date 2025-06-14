using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class ApAgingReportRequest : IRequest<ApAgingReportDto>
{
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow; // Default to today

    // Optional filters:
    public Guid? SupplierId { get; set; }
    // public bool ExcludeFullyPaidInvoices { get; set; } = true; // Common filter
    // public string? Currency { get; set; } // If dealing with multi-currency payables
}
