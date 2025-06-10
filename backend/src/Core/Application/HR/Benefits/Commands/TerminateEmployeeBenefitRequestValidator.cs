using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class TerminateEmployeeBenefitRequestValidator : CustomValidator<TerminateEmployeeBenefitRequest>
{
    public TerminateEmployeeBenefitRequestValidator(IStringLocalizer<TerminateEmployeeBenefitRequestValidator> T)
    {
        RuleFor(p => p.EmployeeBenefitId)
            .NotEmpty();

        RuleFor(p => p.TerminationDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-1).Date) // Example: Not too far in past (e.g. allow backdating up to a year)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(5).Date); // Example: Not too far in future

        RuleFor(p => p.Reason)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.Reason));
    }
}
