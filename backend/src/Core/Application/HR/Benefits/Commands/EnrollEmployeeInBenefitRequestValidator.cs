using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Domain.HR.Benefits; // For BenefitPlan
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

// Minimal EmployeeExistsSpec (can be shared)
public class EmployeeBenefitEmployeeExistsSpec : Specification<Employee>, ISingleResultSpecification
{
    public EmployeeBenefitEmployeeExistsSpec(Guid id) => Query.Where(e => e.Id == id);
}

// Minimal BenefitPlanExistsSpec (can be shared)
public class BenefitPlanIsActiveSpec : Specification<BenefitPlan>, ISingleResultSpecification
{
    public BenefitPlanIsActiveSpec(Guid id) => Query.Where(bp => bp.Id == id && bp.IsActive);
}

public class EnrollEmployeeInBenefitRequestValidator : CustomValidator<EnrollEmployeeInBenefitRequest>
{
    public EnrollEmployeeInBenefitRequestValidator(
        IReadRepository<Employee> employeeRepository,
        IReadRepository<BenefitPlan> benefitPlanRepository,
        IStringLocalizer<EnrollEmployeeInBenefitRequestValidator> T)
    {
        RuleFor(p => p.EmployeeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await employeeRepository.FirstOrDefaultAsync(new EmployeeBenefitEmployeeExistsSpec(id), ct) is not null)
                .WithMessage(T["Employee with ID {0} not found.", (req, id) => id]);

        RuleFor(p => p.BenefitPlanId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await benefitPlanRepository.FirstOrDefaultAsync(new BenefitPlanIsActiveSpec(id), ct) is not null)
                .WithMessage(T["Active Benefit Plan with ID {0} not found or is not active.", (req, id) => id]);

        RuleFor(p => p.EnrollmentDate)
            .NotEmpty();

        RuleFor(p => p.EffectiveDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(p => p.EnrollmentDate)
                .WithMessage(T["Effective Date must be on or after Enrollment Date."]);

        RuleFor(p => p.EmployeeContributionOverride)
            .GreaterThanOrEqualTo(0)
            .When(p => p.EmployeeContributionOverride.HasValue);

        RuleFor(p => p.EmployerContributionOverride)
            .GreaterThanOrEqualTo(0)
            .When(p => p.EmployerContributionOverride.HasValue);

        RuleFor(p => p.Notes)
            .MaximumLength(1000)
            .When(p => !string.IsNullOrEmpty(p.Notes));
    }
}
