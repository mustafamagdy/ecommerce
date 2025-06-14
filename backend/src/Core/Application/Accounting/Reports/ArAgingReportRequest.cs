using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class ArAgingReportRequest : IRequest<ArAgingReportDto>
{
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow; // Default to today
    public Guid? CustomerId { get; set; } // Optional filter

    // Optional future filters:
    // public bool ExcludeFullyPaidInvoices { get; set; } = true;
    // public Guid? SalespersonId { get; set; }
    // public string? Region { get; set; }
}
