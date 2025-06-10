using FSH.WebApi.Domain.HR.Enums; // For BenefitPlanType
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class DefineBenefitPlanRequest : IRequest<Guid> // Returns BenefitPlan.Id
{
    public Guid? Id { get; set; } // Nullable for create, non-null for update

    public string PlanName { get; set; } = string.Empty;
    public string? PlanCode { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }
    public BenefitPlanType Type { get; set; }
    public decimal? ContributionAmountEmployee { get; set; }
    public decimal? ContributionAmountEmployer { get; set; }
    public bool IsActive { get; set; } = true; // Default to active for new plans
}
