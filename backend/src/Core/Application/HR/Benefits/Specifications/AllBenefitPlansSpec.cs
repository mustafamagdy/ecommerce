using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class AllBenefitPlansSpec : Specification<BenefitPlan, BenefitPlanDto>
{
    public AllBenefitPlansSpec(bool? isActive = true) // Default to fetching active plans
    {
        if (isActive.HasValue)
        {
            Query.Where(bp => bp.IsActive == isActive.Value);
        }

        Query.OrderBy(bp => bp.PlanName);

        Query.Select(bp => new BenefitPlanDto
        {
            Id = bp.Id,
            PlanName = bp.PlanName,
            PlanCode = bp.PlanCode,
            Description = bp.Description,
            Provider = bp.Provider,
            Type = bp.Type,
            ContributionAmountEmployee = bp.ContributionAmountEmployee,
            ContributionAmountEmployer = bp.ContributionAmountEmployer,
            IsActive = bp.IsActive,
            CreatedOn = bp.CreatedOn,
            LastModifiedOn = bp.LastModifiedOn
        });
    }
}
