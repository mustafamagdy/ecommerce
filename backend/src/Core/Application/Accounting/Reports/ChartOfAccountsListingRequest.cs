using MediatR; // For IRequest
using System.Collections.Generic; // For List if more complex filters are added

namespace FSH.WebApi.Application.Accounting.Reports;

// No specific parameters needed for a full listing initially,
// but could add filters like IsActive later.
public class ChartOfAccountsListingRequest : IRequest<ChartOfAccountsListingDto>
{
    public bool? IsActive { get; set; } // Optional filter
    // public string? AccountType { get; set; } // Optional filter by specific account type
    // public Guid? ParentAccountId { get; set; } // Optional filter for children of a specific account
}
