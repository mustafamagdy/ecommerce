using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using FluentValidation;
using Microsoft.Extensions.Localization;

using FSH.WebApi.Application.HR.Payroll.Specifications; // For EmployeeExistsSpec, SalaryStructureByEmployeeIdSpec

namespace FSH.WebApi.Application.HR.Payroll;

public class GeneratePayslipRequestValidator : CustomValidator<GeneratePayslipRequest>
{
    public GeneratePayslipRequestValidator(
        IReadRepository<Employee> employeeRepository, // To check if EmployeeId exists
        IReadRepository<SalaryStructure> salaryStructureRepository, // To check if SalaryStructure exists for Employee
        IStringLocalizer<GeneratePayslipRequestValidator> T)
    {
        RuleFor(p => p.EmployeeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new EmployeeExistsSpec(id), ct) is not null)
                .WithMessage(T["Employee with ID {0} not found.", (req, id) => id])
            .MustAsync(async (id, ct) => await salaryStructureRepository.FirstOrDefaultAsync(new SalaryStructureByEmployeeIdSpec(id), ct) is not null)
                .WithMessage(T["Salary Structure for Employee ID {0} not found. Please define it first.", (req, id) => id]);


        RuleFor(p => p.PayPeriodStartDate)
            .NotEmpty()
            .LessThan(p => p.PayPeriodEndDate)
                .WithMessage(T["Pay period start date must be before the end date."]);

        RuleFor(p => p.PayPeriodEndDate)
            .NotEmpty()
            .GreaterThan(p => p.PayPeriodStartDate)
                .WithMessage(T["Pay period end date must be after the start date."]);

        // Additional rule: Ensure the pay period is reasonable, e.g., not spanning multiple years, or is typically a month.
        // For example, ensure EndDate is not more than X days/months after StartDate.
        RuleFor(p => p.PayPeriodEndDate)
            .Must((req, endDate) => (endDate - req.PayPeriodStartDate).TotalDays <= 35) // Approx 1 month + a few days buffer
                .WithMessage(T["Pay period duration is too long. It should typically be around a month."])
            .When(p => p.PayPeriodStartDate != DateTime.MinValue && p.PayPeriodEndDate != DateTime.MinValue);
    }
}
