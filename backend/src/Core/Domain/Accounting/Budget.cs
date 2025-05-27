using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class Budget : AuditableEntity, IAggregateRoot
{
    public string BudgetName { get; private set; } = default!;
    public Guid AccountId { get; private set; }
    public DateTime PeriodStartDate { get; private set; }
    public DateTime PeriodEndDate { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public virtual Account Account { get; set; } = default!;

    public Budget(string budgetName, Guid accountId, DateTime periodStartDate, DateTime periodEndDate, decimal amount, string? description)
    {
        BudgetName = budgetName;
        AccountId = accountId;
        PeriodStartDate = periodStartDate;
        PeriodEndDate = periodEndDate;
        Amount = amount;
        Description = description;
    }

    public Budget Update(string? budgetName, Guid? accountId, DateTime? periodStartDate, DateTime? periodEndDate, decimal? amount, string? description)
    {
        if (budgetName is not null && BudgetName?.Equals(budgetName) is not true) BudgetName = budgetName;
        if (accountId.HasValue && AccountId != accountId.Value) AccountId = accountId.Value;
        if (periodStartDate.HasValue && PeriodStartDate != periodStartDate.Value) PeriodStartDate = periodStartDate.Value;
        if (periodEndDate.HasValue && PeriodEndDate != periodEndDate.Value) PeriodEndDate = periodEndDate.Value;
        if (amount.HasValue && Amount != amount.Value) Amount = amount.Value;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        return this;
    }
}
