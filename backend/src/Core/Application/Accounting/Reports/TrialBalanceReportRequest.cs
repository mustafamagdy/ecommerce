using MediatR; // FSH.WebApi.Application.Common.Models if specific base request is used. For now, MediatR directly.
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class TrialBalanceReportRequest : IRequest<TrialBalanceReportDto>
{
    public DateTime EndDate { get; set; } = DateTime.UtcNow; // Default to today, can be overridden
    // Optional: public DateTime? StartDate { get; set; } // For calculating balances over a period, not just as-of EndDate
}
