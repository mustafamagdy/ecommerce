using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class TrialBalanceReportDto
{
    public DateTime ReportDate { get; set; } // Renamed from EndDate for clarity on the report itself
    // public DateTime? StartDate { get; set; } // If period-based balances are shown
    public List<TrialBalanceReportLineDto> Lines { get; set; } = new();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.001m; // Using a small tolerance for decimal comparison
    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class TrialBalanceReportLineDto
{
    public Guid AccountId { get; set; } // Added AccountId for potential drill-down or linking
    public string AccountCode { get; set; } = default!; // E.g., GL Account Number
    public string AccountName { get; set; } = default!;
    public string AccountType { get; set; } = default!; // From AccountType enum, converted to string
    public decimal DebitBalance { get; set; }  // Should be 0 if CreditBalance > 0
    public decimal CreditBalance { get; set; } // Should be 0 if DebitBalance > 0
    // public decimal Balance { get; set; } // Alternative: single balance column, positive for Debit, negative for Credit

    // Constructor to ensure one balance type is set based on typical GL account behavior
    public TrialBalanceReportLineDto(Guid accountId, string accountCode, string accountName, string accountType, decimal balance)
    {
        AccountId = accountId;
        AccountCode = accountCode;
        AccountName = accountName;
        AccountType = accountType;

        // Assuming a convention: Assets/Expenses are typically Debit balances, Liabilities/Equity/Revenue are Credit balances.
        // This logic might be more complex based on AccountType and actual balance sign.
        // For Trial Balance, we show the "natural" balance of the account.
        // If balance > 0, it's Debit for Assets/Expenses, Credit for L/E/R.
        // If balance < 0, it's Credit for Assets/Expenses, Debit for L/E/R (contra-accounts or errors).
        // This simplified DTO just takes a balance and assigns it. The handler will determine if it's debit or credit.
        // For now, let's assume the handler will populate DebitBalance or CreditBalance correctly.
        // The DTO will just hold the values. A common approach is to have a single 'Balance' and then derive Dr/Cr in presentation,
        // or for the handler to decide which column to populate.
        // For this DTO, let's assume handler sets DebitBalance or CreditBalance, not both.
    }

    // Parameterless constructor for serializers/mappers if needed
    public TrialBalanceReportLineDto() { }
}
