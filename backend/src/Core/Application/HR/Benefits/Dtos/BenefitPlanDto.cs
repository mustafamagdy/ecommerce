using FSH.WebApi.Domain.HR.Enums; // For BenefitPlanType

namespace FSH.WebApi.Application.HR.Benefits.Dtos;

public class BenefitPlanDto
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? PlanCode { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }

    public BenefitPlanType Type { get; set; }
    public string TypeDescription => Type.ToString(); // Calculated property

    public decimal? ContributionAmountEmployee { get; set; }
    public decimal? ContributionAmountEmployer { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
