using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For ApplicantStatus
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

public class UpdateApplicantStatusRequestValidator : CustomValidator<UpdateApplicantStatusRequest>
{
    public UpdateApplicantStatusRequestValidator(IStringLocalizer<UpdateApplicantStatusRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum()
            .WithMessage(T["Invalid Applicant Status."]);

        RuleFor(p => p.Notes)
            .MaximumLength(1000) // Example max length for notes
            .When(p => p.Notes is not null);
    }
}
