using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class BudgetDto : IDto
{
    public Guid Id { get; set; }
    public string BudgetName { get; set; } = default!;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = default!; // For display
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal Amount { get; set; } // Budgeted Amount
    public decimal ActualAmount { get; set; } // Calculated
    public decimal Variance { get; set; } // Calculated (Amount - ActualAmount)
    public string? Description { get; set; }
}
