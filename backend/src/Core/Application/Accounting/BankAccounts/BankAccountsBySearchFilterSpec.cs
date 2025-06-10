using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting; // For BankAccount
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class BankAccountsBySearchFilterSpec : EntitiesByPaginationFilterSpec<BankAccount, BankAccountDto>
{
    public BankAccountsBySearchFilterSpec(SearchBankAccountsRequest request)
        : base(request)
    {
        Query.OrderBy(ba => ba.AccountName, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.NameKeyword))
        {
            Query.Search(ba => ba.AccountName, "%" + request.NameKeyword + "%")
                 .Search(ba => ba.AccountNumber, "%" + request.NameKeyword + "%")
                 .Search(ba => ba.BankName, "%" + request.NameKeyword + "%");
        }

        if (!string.IsNullOrEmpty(request.Currency))
        {
            Query.Where(ba => ba.Currency == request.Currency);
        }

        if (request.GLAccountId.HasValue)
        {
            Query.Where(ba => ba.GLAccountId == request.GLAccountId.Value);
        }

        if (request.IsActive.HasValue)
        {
            Query.Where(ba => ba.IsActive == request.IsActive.Value);
        }

        // Similar to BankAccountByIdSpec, if GLAccount was a direct navigation property and needed in search results:
        // Query.Include(ba => ba.GLAccount);
        // However, for list views, fetching extra details for each item can be costly.
        // Populating GLAccountCode/Name will be done in the handler.
    }
}
