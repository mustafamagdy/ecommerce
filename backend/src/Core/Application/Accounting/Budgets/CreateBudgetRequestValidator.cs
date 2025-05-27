using FluentValidation;
using FSH.WebApi.Application.Common.Validation;
using System;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class CreateBudgetRequestValidator : CustomValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(p => p.BudgetName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(p => p.AccountId)
            .NotEmpty();

        RuleFor(p => p.PeriodStartDate)
            .NotEmpty();

        RuleFor(p => p.PeriodEndDate)
            .NotEmpty()
            .GreaterThan(p => p.PeriodStartDate)
                .WithMessage("Period End Date must be after Period Start Date.");

        RuleFor(p => p.Amount)
            .GreaterThan(0)
                .WithMessage("Amount must be positive.");
    }
}
