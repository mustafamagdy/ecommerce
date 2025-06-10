using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus

namespace FSH.WebApi.Application.HR.Benefits.Dtos;

public class EmployeeBenefitDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; } // To be populated (FirstName LastName)

    public Guid BenefitPlanId { get; set; }
    public string? BenefitPlanName { get; set; } // To be populated (BenefitPlan.PlanName)

    public DateTime EnrollmentDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    public decimal EmployeeContributionActual { get; set; } // Resolved value
    public decimal EmployerContributionActual { get; set; } // Resolved value

    public EmployeeBenefitStatus Status { get; set; }
    public string StatusDescription => Status.ToString(); // Calculated property

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
