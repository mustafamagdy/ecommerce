using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class UpdateEmployeeBenefitRequestValidator : CustomValidator<UpdateEmployeeBenefitRequest>
{
    public UpdateEmployeeBenefitRequestValidator(IStringLocalizer<UpdateEmployeeBenefitRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.EnrollmentDate)
            .NotEmpty()
            .When(p => p.EnrollmentDate.HasValue);

        RuleFor(p => p.EffectiveDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.EnrollmentDate!.Value) // Assumes if EffectiveDate is set, EnrollmentDate is relevant
                .WithMessage(T["Effective Date must be on or after Enrollment Date."])
            .When(p => p.EffectiveDate.HasValue && p.EnrollmentDate.HasValue); // Only if both are being set or one is set and other exists

        RuleFor(p => p.TerminationDate)
            .GreaterThanOrEqualTo(p => p.EffectiveDate!.Value) // Assumes EffectiveDate is relevant
                .WithMessage(T["Termination Date must be on or after Effective Date."])
            .When(p => p.TerminationDate.HasValue && p.EffectiveDate.HasValue);

        RuleFor(p => p.EmployeeContributionOverride)
            .GreaterThanOrEqualTo(0)
            .When(p => p.EmployeeContributionOverride.HasValue);

        RuleFor(p => p.EmployerContributionOverride)
            .GreaterThanOrEqualTo(0)
            .When(p => p.EmployerContributionOverride.HasValue);

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum()
            .When(p => p.Status.HasValue);

        RuleFor(p => p.Notes)
            .MaximumLength(1000)
            .When(p => p.Notes is not null); // Allow clearing notes by sending null
    }
}
