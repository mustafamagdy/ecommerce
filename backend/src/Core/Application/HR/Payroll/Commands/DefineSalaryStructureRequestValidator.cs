using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using FluentValidation;
using Microsoft.Extensions.Localization;

using FSH.WebApi.Application.HR.Payroll.Specifications; // For EmployeeExistsSpec

namespace FSH.WebApi.Application.HR.Payroll;

public class DefineSalaryStructureRequestValidator : CustomValidator<DefineSalaryStructureRequest>
{
    public DefineSalaryStructureRequestValidator(
        IReadRepository<Employee> employeeRepository,
        IStringLocalizer<DefineSalaryStructureRequestValidator> T)
    {
        RuleFor(p => p.EmployeeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new EmployeeExistsSpec(id), ct) is not null)
                .WithMessage(T["Employee with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.BasicSalary)
            .GreaterThan(0)
                .WithMessage(T["Basic Salary must be greater than 0."]);

        RuleForEach(p => p.Earnings).ChildRules(earnings =>
        {
            earnings.RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
            earnings.RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        });

        RuleForEach(p => p.Deductions).ChildRules(deductions =>
        {
            deductions.RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
            deductions.RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        });
    }
}
