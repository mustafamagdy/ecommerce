using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Application.HR.Benefits.Dtos;

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class EmployeeBenefitsByEmployeeIdSpec : Specification<EmployeeBenefit, EmployeeBenefitDto>
{
    public EmployeeBenefitsByEmployeeIdSpec(Guid employeeId)
    {
        Query
            .Where(eb => eb.EmployeeId == employeeId)
            .Include(eb => eb.Employee)
            .Include(eb => eb.BenefitPlan)
            .OrderByDescending(eb => eb.EffectiveDate); // Show most recent first

        Query.Select(eb => new EmployeeBenefitDto
        {
            Id = eb.Id,
            EmployeeId = eb.EmployeeId,
            EmployeeName = eb.Employee != null ? $"{eb.Employee.FirstName} {eb.Employee.LastName}" : null,
            BenefitPlanId = eb.BenefitPlanId,
            BenefitPlanName = eb.BenefitPlan != null ? eb.BenefitPlan.PlanName : null,
            EnrollmentDate = eb.EnrollmentDate,
            EffectiveDate = eb.EffectiveDate,
            TerminationDate = eb.TerminationDate,
            EmployeeContributionActual = eb.GetEmployeeContribution(), // Use helper
            EmployerContributionActual = eb.GetEmployerContribution(), // Use helper
            Status = eb.Status, // StatusDescription is on DTO
            Notes = eb.Notes,
            CreatedOn = eb.CreatedOn,
            LastModifiedOn = eb.LastModifiedOn
        });
    }
}
