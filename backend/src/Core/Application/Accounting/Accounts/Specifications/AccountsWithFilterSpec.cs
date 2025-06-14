using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For Account
using System.Linq; // Required for OrderBy

namespace FSH.WebApi.Application.Accounting.Accounts.Specifications;

// This specification is for fetching Account entities, not DTOs directly from query.
public class AccountsWithFilterSpec : Specification<Account>
{
    public AccountsWithFilterSpec(bool? isActive)
    {
        Query.OrderBy(a => a.AccountNumber); // Default sort by AccountNumber (Code)

        if (isActive.HasValue)
        {
            Query.Where(a => a.IsActive == isActive.Value);
        }
        // Could add other filters here if needed in the future, e.g., by AccountType
        // Query.Include(a => a.ParentAccount); // Optional: if ParentAccount navigation property exists and is needed
    }
}
