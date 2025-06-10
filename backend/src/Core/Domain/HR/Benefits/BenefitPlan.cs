using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.HR.Enums; // For BenefitPlanType

namespace FSH.WebApi.Domain.HR.Benefits;

public class BenefitPlan : AuditableEntity
{
    public string PlanName { get; set; } = string.Empty;
    public string? PlanCode { get; set; } // e.g., HLTGLD01, DENPREM
    public string? Description { get; set; }
    public string? Provider { get; set; } // e.g., "Global Health Inc.", "SecureRetire Co."

    public BenefitPlanType Type { get; set; }

    // Standard contribution amounts defined at the plan level (per pay period or monthly, policy defined)
    public decimal? ContributionAmountEmployee { get; set; }
    public decimal? ContributionAmountEmployer { get; set; }

    public bool IsActive { get; set; } = true; // To allow deactivating old plans not offered anymore

    public BenefitPlan(string planName, BenefitPlanType type)
    {
        PlanName = planName;
        Type = type;
    }

    // Parameterless constructor for EF Core
    private BenefitPlan() { }
}
