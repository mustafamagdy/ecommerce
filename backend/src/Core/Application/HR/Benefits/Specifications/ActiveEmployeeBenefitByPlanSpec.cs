using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class ActiveEmployeeBenefitByPlanSpec : Specification<EmployeeBenefit>, ISingleResultSpecification
{
    public ActiveEmployeeBenefitByPlanSpec(Guid employeeId, Guid benefitPlanId)
    {
        Query
            .Where(eb => eb.EmployeeId == employeeId &&
                         eb.BenefitPlanId == benefitPlanId &&
                         eb.Status == EmployeeBenefitStatus.Active);
    }
}
