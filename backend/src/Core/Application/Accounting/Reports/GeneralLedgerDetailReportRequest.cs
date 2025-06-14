using MediatR; // For IRequest
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class GeneralLedgerDetailReportRequest : IRequest<GeneralLedgerDetailReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } = DateTime.UtcNow; // Default EndDate to today

    // AccountId is crucial for a typical GL Detail report.
    // Making it non-nullable to enforce selection, but can be changed if "all accounts" GL is a use case.
    [ValidateNotEmpty] // Assuming this custom attribute or a FluentValidation rule will enforce it.
    public Guid AccountId { get; set; }

    // Add other potential filters like JournalEntrySource, UserId etc. later if needed
    // public string? JournalEntrySource { get; set; }
}
