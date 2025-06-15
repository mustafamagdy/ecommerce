using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CreditMemo, CreditMemoStatus
using FSH.WebApi.Domain.Operation.Customers; // For Customer
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CreditMemos.Specifications;

public class CreditMemosForRegisterSpec : Specification<CreditMemo>
{
    public CreditMemosForRegisterSpec(DateTime? startDate, DateTime? endDate, Guid? customerId, CreditMemoRegisterStatusFilter statusFilter)
    {
        Query
            .Include(cm => cm.Customer) // For CustomerName
            .Include(cm => cm.Applications); // For calculating AppliedAmount and AvailableBalance

        if (startDate.HasValue)
        {
            Query.Where(cm => cm.Date >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            Query.Where(cm => cm.Date <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }
        if (customerId.HasValue)
        {
            Query.Where(cm => cm.CustomerId == customerId.Value);
        }

        if (statusFilter != CreditMemoRegisterStatusFilter.All)
        {
            // Convert enum to domain status for filtering.
            // This requires mapping the report filter enum to the domain enum.
            // The domain CreditMemoStatus is: Draft, Approved, PartiallyApplied, Applied, Void.
            // The report filter CreditMemoRegisterStatusFilter is: All, Draft, Approved, PartiallyApplied, Applied, Void.
            // Direct mapping is possible here.
            if (Enum.TryParse<CreditMemoStatus>(statusFilter.ToString(), true, out var domainStatus))
            {
                // Special handling for "Approved" in filter which means Approved OR PartiallyApplied but not fully Applied.
                // However, the domain entity's GetAvailableBalance() and GetAppliedAmount() combined with its Status
                // will be used by the handler to further refine this if needed.
                // For now, a direct status filter is a good first pass.
                // The handler will need to be smart about the "Approved" filter meaning "has available balance".
                // For this spec, just filter by the direct status if it's not "All".
                // The handler can do more refined filtering based on calculated balances later.
                Query.Where(cm => cm.Status == domainStatus);
            }
        }

        Query.OrderByDescending(cm => cm.Date).ThenBy(cm => cm.Customer.Name); // Default sort
    }
}
