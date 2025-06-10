using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For BenefitPlanType
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class DefineBenefitPlanRequestValidator : CustomValidator<DefineBenefitPlanRequest>
{
    public DefineBenefitPlanRequestValidator(IStringLocalizer<DefineBenefitPlanRequestValidator> T)
    {
        // ID can be null for create, must be present for update (though handler logic implies this)
        // RuleFor(p => p.Id).NotEmpty().When(p => p.Id.HasValue); // Or let handler manage this distinction

        RuleFor(p => p.PlanName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.PlanCode)
            .MaximumLength(50)
            .When(p => !string.IsNullOrEmpty(p.PlanCode));

        RuleFor(p => p.Description)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.Description));

        RuleFor(p => p.Provider)
            .MaximumLength(100)
            .When(p => !string.IsNullOrEmpty(p.Provider));

        RuleFor(p => p.Type)
            .NotNull()
            .IsInEnum();

        RuleFor(p => p.ContributionAmountEmployee)
            .GreaterThanOrEqualTo(0)
            .When(p => p.ContributionAmountEmployee.HasValue);

        RuleFor(p => p.ContributionAmountEmployer)
            .GreaterThanOrEqualTo(0)
            .When(p => p.ContributionAmountEmployer.HasValue);

        RuleFor(p => p.IsActive)
            .NotNull(); // bool is not nullable by default
    }
}
