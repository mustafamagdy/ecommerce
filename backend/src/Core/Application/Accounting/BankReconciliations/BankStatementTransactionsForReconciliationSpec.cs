using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction, BankTransactionType
using System;
using LinqKit; // For PredicateBuilder if complex filtering is needed
using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

// Using EntitiesByPaginationFilterSpec for consistency
public class BankStatementTransactionsForReconciliationSpec : EntitiesByPaginationFilterSpec<BankStatementTransaction, BankStatementTransactionDto>
{
    public BankStatementTransactionsForReconciliationSpec(Guid bankStatementId, GetBankStatementTransactionsForReconciliationRequest request)
        : base(request) // Pass the pagination request to the base
    {
        Query.Where(tx => tx.BankStatementId == bankStatementId);

        switch (request.FilterStatus)
        {
            case ReconciliationTransactionFilterStatus.Matched:
                Query.Where(tx => tx.IsReconciled == true && tx.BankReconciliationId == request.BankReconciliationId);
                break;
            case ReconciliationTransactionFilterStatus.Unmatched:
                Query.Where(tx => tx.IsReconciled == false);
                break;
            case ReconciliationTransactionFilterStatus.All:
            default:
                // No additional status filter needed
                break;
        }

        if (request.TransactionType.HasValue)
        {
            Query.Where(tx => tx.Type == request.TransactionType.Value);
        }

        if (request.DateFrom.HasValue)
        {
            Query.Where(tx => tx.TransactionDate >= request.DateFrom.Value);
        }
        if (request.DateTo.HasValue)
        {
            Query.Where(tx => tx.TransactionDate <= request.DateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrEmpty(request.DescriptionKeyword))
        {
            Query.Search(tx => tx.Description, "%" + request.DescriptionKeyword + "%");
        }

        if (request.ExactAmount.HasValue)
        {
            Query.Where(tx => tx.Amount == request.ExactAmount.Value);
        }

        if (!request.HasOrderBy()) // Apply default order if none specified in request
        {
            Query.OrderBy(tx => tx.TransactionDate).ThenBy(tx => tx.CreatedOn);
        }
        // No specific Includes needed here if BankStatementTransactionDto is flat.
        // If it needed, e.g., SystemTransaction details from a navigated property, Include would go here.
    }
}
