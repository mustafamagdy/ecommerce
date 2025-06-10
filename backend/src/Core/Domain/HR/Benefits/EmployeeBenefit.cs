using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus

namespace FSH.WebApi.Domain.HR.Benefits;

public class EmployeeBenefit : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    public Guid BenefitPlanId { get; set; }
    public virtual BenefitPlan? BenefitPlan { get; set; }

    public DateTime EnrollmentDate { get; set; } // When the employee enrolled or election was made
    public DateTime EffectiveDate { get; set; }  // When the benefit coverage starts
    public DateTime? TerminationDate { get; set; } // When the benefit coverage ends for the employee

    // Overrides for contributions if specific to this employee's enrollment
    public decimal? EmployeeContributionOverride { get; set; }
    public decimal? EmployerContributionOverride { get; set; }

    public EmployeeBenefitStatus Status { get; set; } = EmployeeBenefitStatus.PendingEnrollment;
    public string? Notes { get; set; }

    public EmployeeBenefit(Guid employeeId, Guid benefitPlanId, DateTime enrollmentDate, DateTime effectiveDate)
    {
        EmployeeId = employeeId;
        BenefitPlanId = benefitPlanId;
        EnrollmentDate = enrollmentDate;
        EffectiveDate = effectiveDate;
    }

    // Parameterless constructor for EF Core
    private EmployeeBenefit() { }

    // Helper to get current employee contribution (considering override)
    public decimal GetEmployeeContribution()
    {
        return EmployeeContributionOverride ?? BenefitPlan?.ContributionAmountEmployee ?? 0;
    }

    // Helper to get current employer contribution (considering override)
    public decimal GetEmployerContribution()
    {
        return EmployerContributionOverride ?? BenefitPlan?.ContributionAmountEmployer ?? 0;
    }
}
