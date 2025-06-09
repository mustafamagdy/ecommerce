using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.Common.Persistence;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Employees.Commands;

// Spec for checking email uniqueness, excluding a specific employee ID
public class EmployeeByEmailExceptIdSpec : Specification<Employee>, ISingleResultSpecification
{
    public EmployeeByEmailExceptIdSpec(string email, Guid employeeIdToExclude) =>
        Query.Where(e => e.Email.ToLower() == email.ToLower() && e.Id != employeeIdToExclude);
}

public class UpdateEmployeeRequestValidator : CustomValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator(
        IReadRepository<Employee> employeeRepository,
        IReadRepository<Department> departmentRepository,
        IReadRepository<JobTitle> jobTitleRepository,
        IStringLocalizer<UpdateEmployeeRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(75)
            .When(p => p.FirstName is not null); // Only validate if provided

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(75)
            .When(p => p.LastName is not null); // Only validate if provided

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid email format."])
            .MaximumLength(256)
            .MustAsync(async (request, email, ct) =>
                await employeeRepository.FirstOrDefaultAsync(new EmployeeByEmailExceptIdSpec(email!, request.Id), ct) is null)
                .WithMessage((request, email) => T["Employee with email {0} already exists for another user.", email])
            .When(p => p.Email is not null); // Only validate if provided

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(20)
            .When(p => p.PhoneNumber is not null || p.PhoneNumber == string.Empty); // Allow clearing

        RuleFor(p => p.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-16))
                .WithMessage(T["Employee must be at least 16 years old."])
            .When(p => p.DateOfBirth.HasValue);

        // Assuming DateOfJoining might not be updatable or has specific rules if it is.
        // If updatable, similar rules as Create, considering existing DateOfBirth.
        RuleFor(p => p.DateOfJoining)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage(T["Date of joining cannot be in the distant future."])
            // .GreaterThanOrEqualTo(p => p.DateOfBirth.Value.AddYears(16)) // This needs Employee's current DOB
            //     .WithMessage(T["Date of joining must be at least 16 years after Date of Birth."])
            .When(p => p.DateOfJoining.HasValue);


        RuleFor(p => p.DepartmentId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await departmentRepository.FirstOrDefaultAsync(new DepartmentByIdSpec(id!.Value), ct) is not null)
                .WithMessage((_, id) => T["Department with ID {0} not found.", id])
            .When(p => p.DepartmentId.HasValue);

        RuleFor(p => p.JobTitleId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await jobTitleRepository.FirstOrDefaultAsync(new JobTitleByIdSpec(id!.Value), ct) is not null)
                .WithMessage((_, id) => T["Job Title with ID {0} not found.", id])
            .When(p => p.JobTitleId.HasValue);

        RuleFor(p => p.ManagerId)
            .MustAsync(async (managerId, ct) => // Check 1: Proposed manager must exist
            {
                if (!managerId.HasValue) return true; // No manager assigned, so no issue
                return await employeeRepository.GetByIdAsync(managerId.Value, ct) is not null;
            })
                .WithMessage((_, id) => T["Proposed manager with ID {0} not found.", id])
            .NotEqual(p => p.Id)
                .WithMessage(T["An employee cannot be their own manager."])
            .MustAsync(async (request, managerId, ct) => // Check 2: Prevent direct circular dependency
            {
                if (!managerId.HasValue) return true; // No manager, no circular dependency
                var proposedManager = await employeeRepository.GetByIdAsync(managerId.Value, ct);
                // If proposedManager is null, previous rule handles it.
                // If proposedManager's manager is the current employee being updated, it's a circular dependency.
                if (proposedManager?.ManagerId == request.Id) return false;
                return true;
            })
                .WithMessage(T["This assignment creates a direct circular manager dependency."])
            .When(p => p.ManagerId.HasValue); // All these rules apply only if ManagerId is being set
    }
}
