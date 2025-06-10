using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For BankStatement
using System;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class BankStatementsBySearchFilterSpec : EntitiesByPaginationFilterSpec<BankStatement, BankStatementDto>
{
    public BankStatementsBySearchFilterSpec(SearchBankStatementsRequest request)
        : base(request)
    {
        Query.OrderByDescending(bs => bs.StatementDate, !request.HasOrderBy()); // Default order

        if (request.BankAccountId.HasValue)
        {
            Query.Where(bs => bs.BankAccountId == request.BankAccountId.Value);
        }

        if (request.StatementDateFrom.HasValue)
        {
            Query.Where(bs => bs.StatementDate >= request.StatementDateFrom.Value);
        }
        if (request.StatementDateTo.HasValue)
        {
            Query.Where(bs => bs.StatementDate <= request.StatementDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrEmpty(request.ReferenceNumberKeyword))
        {
            Query.Search(bs => bs.ReferenceNumber, "%" + request.ReferenceNumberKeyword + "%");
        }

        if (request.ImportDateFrom.HasValue)
        {
            Query.Where(bs => bs.ImportDate >= request.ImportDateFrom.Value);
        }
        if (request.ImportDateTo.HasValue)
        {
            Query.Where(bs => bs.ImportDate <= request.ImportDateTo.Value.AddDays(1).AddTicks(-1));
        }

        // Include BankAccount for populating BankAccountName in DTOs for the list view.
        // Transactions are generally not included in list views unless very summarized.
        // Full transactions list is usually for the detail view (GetBankStatementHandler).
        Query.Include(bs => bs.BankAccount);
    }
}
