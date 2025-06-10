using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class CancelInterviewRequestValidator : CustomValidator<CancelInterviewRequest>
{
    public CancelInterviewRequestValidator(IStringLocalizer<CancelInterviewRequestValidator> T)
    {
        RuleFor(p => p.InterviewId)
            .NotEmpty();

        RuleFor(p => p.CancellationReason)
            .MaximumLength(500).WithMessage(T["Cancellation reason must not exceed 500 characters."])
            .When(p => !string.IsNullOrEmpty(p.CancellationReason));
    }
}
