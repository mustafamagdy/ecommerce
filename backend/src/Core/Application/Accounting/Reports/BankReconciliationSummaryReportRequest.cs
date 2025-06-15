using MediatR;
using System;
using FSH.WebApi.Application.Common.Validation; // For [ValidateNotEmpty] if used

namespace FSH.WebApi.Application.Accounting.Reports;

public class BankReconciliationSummaryReportRequest : IRequest<BankReconciliationSummaryReportDto>
{
    [ValidateNotEmpty] // Assuming this custom attribute or a FluentValidation rule will enforce it.
    public Guid BankReconciliationId { get; set; }

    // Alternative lookup methods (could be separate request types or logic in one handler):
    // public Guid? BankAccountId { get; set; }
    // public DateTime? ReconciliationDate { get; set; } // To find a specific reconciliation by date for an account
                                                      // This would require handler logic to find the "latest" or "exact" match.
                                                      // For now, focusing on direct lookup by BankReconciliationId.
}
