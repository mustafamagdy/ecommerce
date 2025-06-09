using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Department
using FSH.WebApi.Domain.HR.Enums; // For JobOpeningStatus
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningRequestValidator : CustomValidator<UpdateJobOpeningRequest>
{
    public UpdateJobOpeningRequestValidator(
        IReadRepository<Department> departmentRepository,
        IStringLocalizer<UpdateJobOpeningRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Title)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.Title is not null);

        RuleFor(p => p.Description)
            .NotEmpty()
            .When(p => p.Description is not null);

        RuleFor(p => p.DepartmentId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await departmentRepository.FirstOrDefaultAsync(new DepartmentExistsSpec(id!.Value), ct) is not null)
                .WithMessage((_, id) => T["Department with ID {0} not found.", id])
            .When(p => p.DepartmentId.HasValue);

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum()
            .When(p => p.Status.HasValue);

        RuleFor(p => p.PostedDate)
            .NotEmpty()
            .When(p => p.PostedDate.HasValue);

        RuleFor(p => p.ClosingDate)
            .GreaterThanOrEqualTo(p => p.PostedDate!.Value) // Assumes PostedDate will be present if ClosingDate is
                .WithMessage(T["Closing Date must be on or after Posted Date."])
            .When(p => p.ClosingDate.HasValue && p.PostedDate.HasValue);
    }
}
