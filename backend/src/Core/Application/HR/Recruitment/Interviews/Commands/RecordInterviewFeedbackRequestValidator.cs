using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For InterviewStatus, ApplicantStatus
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RecordInterviewFeedbackRequestValidator : CustomValidator<RecordInterviewFeedbackRequest>
{
    public RecordInterviewFeedbackRequestValidator(IStringLocalizer<RecordInterviewFeedbackRequestValidator> T)
    {
        RuleFor(p => p.Id) // Interview Id
            .NotEmpty();

        RuleFor(p => p.Feedback)
            .NotEmpty()
            .MaximumLength(4000); // Assuming feedback can be extensive

        RuleFor(p => p.InterviewNewStatus)
            .NotNull()
            .IsInEnum()
            .Must(status => status == InterviewStatus.Completed) // Typically feedback implies completion
                .WithMessage(T["Interview status must be 'Completed' when recording feedback."]);

        RuleFor(p => p.NextRecommendedApplicantStep)
            .NotNull()
            .IsInEnum()
            .When(p => p.NextRecommendedApplicantStep.HasValue);
    }
}
