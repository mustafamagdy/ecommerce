using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class ChartOfAccountsListingDto
{
    public List<ChartOfAccountsListingLineDto> Accounts { get; set; } = new();
    public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
    public int TotalCount => Accounts.Count;
    public bool? FilterIsActive { get; set; } // To reflect the filter applied
}

public class ChartOfAccountsListingLineDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = default!; // Changed from Code to match Account entity property
    public string AccountName { get; set; } = default!;  // Changed from Name to match Account entity property
    public string AccountType { get; set; } = default!; // Enum to string (e.g., from Account.AccountType.ToString())
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountCode { get; set; } // For easier display (from ParentAccount.AccountNumber)
    public string? ParentAccountName { get; set; } // For easier display (from ParentAccount.AccountName)
    public bool IsActive { get; set; }
    public string? Description { get; set; } // Changed from Notes to match Account entity property "Description"
    public int Level { get; set; } // For hierarchical display (0 for top-level, 1 for children, etc.)
    public decimal Balance { get; set; } // Current balance of the account (optional, but useful for CoA views)

    // Parameterless constructor for mappers/serializers
    public ChartOfAccountsListingLineDto() { }

    // Example constructor if direct instantiation is useful (though mapping is more common)
    public ChartOfAccountsListingLineDto(
        Guid id,
        string accountNumber,
        string accountName,
        string accountType,
        bool isActive,
        string? description,
        Guid? parentAccountId = null,
        string? parentAccountCode = null,
        string? parentAccountName = null,
        int level = 0,
        decimal balance = 0m)
    {
        Id = id;
        AccountNumber = accountNumber;
        AccountName = accountName;
        AccountType = accountType;
        IsActive = isActive;
        Description = description;
        ParentAccountId = parentAccountId;
        ParentAccountCode = parentAccountCode;
        ParentAccountName = parentAccountName;
        Level = level;
        Balance = balance;
    }
}
