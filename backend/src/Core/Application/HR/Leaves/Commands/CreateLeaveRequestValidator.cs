using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee, LeaveType
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

// Minimal EmployeeByIdSpec for existence check
public class EmployeeExistsSpec : Specification<Employee>, ISingleResultSpecification
{
    public EmployeeExistsSpec(Guid id) => Query.Where(e => e.Id == id);
}

// Minimal LeaveTypeByIdSpec for existence check
public class LeaveTypeExistsSpec : Specification<LeaveType>, ISingleResultSpecification
{
    public LeaveTypeExistsSpec(Guid id) => Query.Where(lt => lt.Id == id);
}


public class CreateLeaveRequestValidator : CustomValidator<CreateLeaveRequest>
{
    public CreateLeaveRequestValidator(
        IReadRepository<Employee> employeeRepository,
        IReadRepository<LeaveType> leaveTypeRepository,
        IStringLocalizer<CreateLeaveRequestValidator> T)
    {
        RuleFor(p => p.EmployeeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new EmployeeExistsSpec(id), ct) is not null)
                .WithMessage(T["Employee with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.LeaveTypeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await leaveTypeRepository.FirstOrDefaultAsync(new LeaveTypeExistsSpec(id), ct) is not null)
                .WithMessage(T["LeaveType with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.StartDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date) // Start date cannot be in the past (allow today)
                .WithMessage(T["Start date cannot be in the past."]);

        RuleFor(p => p.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.StartDate)
                .WithMessage(T["End date must be on or after the start date."]);

        RuleFor(p => p.Reason)
            .MaximumLength(500)
                .WithMessage(T["Reason must not exceed 500 characters."]);
            // NotEmpty for Reason can be optional depending on business rules.
            // If required: .NotEmpty().WithMessage(T["Reason is required."]);
    }
}
