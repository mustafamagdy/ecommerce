using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Department
using FSH.WebApi.Domain.HR.Enums; // For JobOpeningStatus
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

// Using a minimal spec for Department existence check
public class DepartmentExistsSpec : Specification<Department>, ISingleResultSpecification
{
    public DepartmentExistsSpec(Guid id) => Query.Where(d => d.Id == id);
}

public class CreateJobOpeningRequestValidator : CustomValidator<CreateJobOpeningRequest>
{
    public CreateJobOpeningRequestValidator(
        IReadRepository<Department> departmentRepository,
        IStringLocalizer<CreateJobOpeningRequestValidator> T)
    {
        RuleFor(p => p.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .NotEmpty();

        RuleFor(p => p.DepartmentId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await departmentRepository.FirstOrDefaultAsync(new DepartmentExistsSpec(id), ct) is not null)
                .WithMessage((_, id) => T["Department with ID {0} not found.", id]);

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum();

        RuleFor(p => p.PostedDate)
            .NotEmpty();

        RuleFor(p => p.ClosingDate)
            .GreaterThanOrEqualTo(p => p.PostedDate)
                .WithMessage(T["Closing Date must be on or after Posted Date."])
            .When(p => p.ClosingDate.HasValue);
    }
}
