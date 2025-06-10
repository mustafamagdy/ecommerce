using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits; // For BenefitPlan entity

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class BenefitPlanByCodeSpec : Specification<BenefitPlan>, ISingleResultSpecification
{
    public BenefitPlanByCodeSpec(string planCode)
    {
        Query.Where(bp => bp.PlanCode != null && bp.PlanCode.ToLower() == planCode.ToLower());
    }
}
