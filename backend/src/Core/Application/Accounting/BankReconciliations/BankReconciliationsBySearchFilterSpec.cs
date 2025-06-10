using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation, ReconciliationStatus
using System;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class BankReconciliationsBySearchFilterSpec : EntitiesByPaginationFilterSpec<BankReconciliation, BankReconciliationDto>
{
    public BankReconciliationsBySearchFilterSpec(SearchBankReconciliationsRequest request)
        : base(request)
    {
        Query.OrderByDescending(br => br.ReconciliationDate, !request.HasOrderBy()); // Default order

        if (request.BankAccountId.HasValue)
        {
            Query.Where(br => br.BankAccountId == request.BankAccountId.Value);
        }

        if (request.ReconciliationDateFrom.HasValue)
        {
            Query.Where(br => br.ReconciliationDate >= request.ReconciliationDateFrom.Value);
        }
        if (request.ReconciliationDateTo.HasValue)
        {
            Query.Where(br => br.ReconciliationDate <= request.ReconciliationDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ReconciliationStatus>(request.Status, true, out var statusEnum))
            {
                Query.Where(br => br.Status == statusEnum);
            }
        }

        if (request.BankStatementId.HasValue)
        {
            Query.Where(br => br.BankStatementId == request.BankStatementId.Value);
        }

        // Include BankAccount for populating BankAccountName in DTOs for the list view.
        // BankStatement details (like reference number) might also be useful here.
        Query.Include(br => br.BankAccount)
             .Include(br => br.BankStatement);
    }
}
