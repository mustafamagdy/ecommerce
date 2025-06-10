using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Application.HR.Benefits.Dtos;

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class EmployeeBenefitByIdSpec : Specification<EmployeeBenefit, EmployeeBenefitDto>, ISingleResultSpecification
{
    public EmployeeBenefitByIdSpec(Guid employeeBenefitId)
    {
        Query
            .Where(eb => eb.Id == employeeBenefitId)
            .Include(eb => eb.Employee)
            .Include(eb => eb.BenefitPlan);

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
            EmployeeContributionActual = eb.GetEmployeeContribution(),
            EmployerContributionActual = eb.GetEmployerContribution(),
            Status = eb.Status,
            Notes = eb.Notes,
            CreatedOn = eb.CreatedOn,
            LastModifiedOn = eb.LastModifiedOn
        });
    }
}
