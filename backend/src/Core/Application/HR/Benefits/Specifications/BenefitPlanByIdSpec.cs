using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;
// Assuming Enums are available globally or via FSH.WebApi.Domain.HR.Enums

namespace FSH.WebApi.Application.HR.Benefits.Specifications;

public class BenefitPlanByIdSpec : Specification<BenefitPlan, BenefitPlanDto>, ISingleResultSpecification
{
    public BenefitPlanByIdSpec(Guid benefitPlanId)
    {
        Query.Where(bp => bp.Id == benefitPlanId);

        Query.Select(bp => new BenefitPlanDto
        {
            Id = bp.Id,
            PlanName = bp.PlanName,
            PlanCode = bp.PlanCode,
            Description = bp.Description,
            Provider = bp.Provider,
            Type = bp.Type, // TypeDescription is on DTO
            ContributionAmountEmployee = bp.ContributionAmountEmployee,
            ContributionAmountEmployer = bp.ContributionAmountEmployer,
            IsActive = bp.IsActive,
            CreatedOn = bp.CreatedOn,
            LastModifiedOn = bp.LastModifiedOn
        });
    }
}
