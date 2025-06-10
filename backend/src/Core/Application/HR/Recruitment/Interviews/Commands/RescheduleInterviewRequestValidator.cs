using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Domain.HR.Enums; // For InterviewType
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

// Reusing InterviewerExistsSpec from ScheduleInterviewRequestValidator's file or assuming common location
// public class InterviewerExistsSpec : Specification<Employee>, ISingleResultSpecification ...

public class RescheduleInterviewRequestValidator : CustomValidator<RescheduleInterviewRequest>
{
    public RescheduleInterviewRequestValidator(
        IReadRepository<Employee> employeeRepository, // To validate NewInterviewerId if provided
        IStringLocalizer<RescheduleInterviewRequestValidator> T)
    {
        RuleFor(p => p.InterviewId)
            .NotEmpty();

        RuleFor(p => p.NewScheduledTime)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow)
                .WithMessage(T["New scheduled time must be in the future."]);

        RuleFor(p => p.NewInterviewerId)
            .NotEmpty() // This would make it mandatory if it's not null. If truly optional, no NotEmpty().
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new InterviewerExistsSpec(id!.Value), ct) is not null)
                .WithMessage(T["New interviewer with ID {0} not found.", (req, id) => id])
            .When(p => p.NewInterviewerId.HasValue);

        RuleFor(p => p.NewLocation)
            .MaximumLength(250)
            .When(p => !string.IsNullOrEmpty(p.NewLocation));

        RuleFor(p => p.RescheduleReason)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.RescheduleReason));

        RuleFor(p => p.NewInterviewType)
            .NotNull()
            .IsInEnum()
            .When(p => p.NewInterviewType.HasValue);
    }
}
