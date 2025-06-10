using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Applicant, Employee
using FSH.WebApi.Domain.HR.Enums; // For InterviewType
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

// Minimal ApplicantExistsSpec (could be shared)
public class ApplicantExistsSpec : Specification<Applicant>, ISingleResultSpecification
{
    public ApplicantExistsSpec(Guid id) => Query.Where(a => a.Id == id);
}

// Minimal EmployeeExistsSpec (could be shared or use one from Payroll specs if suitable)
public class InterviewerExistsSpec : Specification<Employee>, ISingleResultSpecification
{
    public InterviewerExistsSpec(Guid id) => Query.Where(e => e.Id == id);
}

public class ScheduleInterviewRequestValidator : CustomValidator<ScheduleInterviewRequest>
{
    public ScheduleInterviewRequestValidator(
        IReadRepository<Applicant> applicantRepository,
        IReadRepository<Employee> employeeRepository,
        IStringLocalizer<ScheduleInterviewRequestValidator> T)
    {
        RuleFor(p => p.ApplicantId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await applicantRepository.FirstOrDefaultAsync(new ApplicantExistsSpec(id), ct) is not null)
                .WithMessage(T["Applicant with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.InterviewerId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new InterviewerExistsSpec(id), ct) is not null)
                .WithMessage(T["Interviewer with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.ScheduledTime)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow)
                .WithMessage(T["Scheduled time must be in the future."]);

        RuleFor(p => p.Type)
            .NotNull()
            .IsInEnum();

        RuleFor(p => p.Location)
            .MaximumLength(250)
            .When(p => !string.IsNullOrEmpty(p.Location));

        RuleFor(p => p.Notes)
            .MaximumLength(1000)
            .When(p => !string.IsNullOrEmpty(p.Notes));
    }
}
