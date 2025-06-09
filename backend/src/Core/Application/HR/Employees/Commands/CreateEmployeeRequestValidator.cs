using FSH.WebApi.Application.Common.Validation; // For CustomValidator
using FSH.WebApi.Domain.HR; // For Department, JobTitle, Employee
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FluentValidation;
using Microsoft.Extensions.Localization; // For IStringLocalizer

namespace FSH.WebApi.Application.HR.Employees.Commands;

// Spec for checking email uniqueness
public class EmployeeByEmailSpec : Specification<Employee>, ISingleResultSpecification
{
    public EmployeeByEmailSpec(string email) =>
        Query.Where(e => e.Email.ToLower() == email.ToLower());
}

// Spec for checking department existence
public class DepartmentByIdSpec : Specification<Department>, ISingleResultSpecification
{
    public DepartmentByIdSpec(Guid id) => Query.Where(d => d.Id == id);
}

// Spec for checking job title existence
public class JobTitleByIdSpec : Specification<JobTitle>, ISingleResultSpecification
{
    public JobTitleByIdSpec(Guid id) => Query.Where(jt => jt.Id == id);
}

public class CreateEmployeeRequestValidator : CustomValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator(
        IReadRepository<Employee> employeeRepository,
        IReadRepository<Department> departmentRepository,
        IReadRepository<JobTitle> jobTitleRepository,
        IStringLocalizer<CreateEmployeeRequestValidator> T)
    {
        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(T["Invalid email format."])
            .MaximumLength(256)
            .MustAsync(async (email, ct) => await employeeRepository.FirstOrDefaultAsync(new EmployeeByEmailSpec(email), ct) is null)
                .WithMessage((_, email) => T["Employee with email {0} already exists.", email]);

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(20);

        RuleFor(p => p.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-16)) // Example: Ensure employee is at least 16 years old
                .WithMessage(T["Employee must be at least 16 years old."]);

        RuleFor(p => p.DateOfJoining)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)) // Cannot be too far in the future (e.g. allow today or yesterday)
                .WithMessage(T["Date of joining cannot be in the distant future."])
            .GreaterThanOrEqualTo(p => p.DateOfBirth.AddYears(16)) // Must be at least 16 years after DOB
                .WithMessage(T["Date of joining must be at least 16 years after Date of Birth."]);

        RuleFor(p => p.DepartmentId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await departmentRepository.FirstOrDefaultAsync(new DepartmentByIdSpec(id), ct) is not null)
                .WithMessage((_, id) => T["Department with ID {0} not found.", id]);

        RuleFor(p => p.JobTitleId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await jobTitleRepository.FirstOrDefaultAsync(new JobTitleByIdSpec(id), ct) is not null)
                .WithMessage((_, id) => T["Job Title with ID {0} not found.", id]);

        RuleFor(p => p.ManagerId)
            .MustAsync(async (id, ct) => id is null || await employeeRepository.GetByIdAsync(id.Value, ct) is not null)
                .WithMessage((_, id) => T["Manager with ID {0} not found.", id])
            .When(p => p.ManagerId.HasValue); // Only validate if ManagerId is provided
    }
}
