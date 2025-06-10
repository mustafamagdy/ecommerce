using FSH.WebApi.Application.HR.Benefits.Dtos; // For BenefitPlanDto
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetAllBenefitPlansRequest : IRequest<List<BenefitPlanDto>>
{
    public bool? IsActive { get; set; } = true; // Default to true to get active plans

    public GetAllBenefitPlansRequest(bool? isActive = true)
    {
        IsActive = isActive;
    }
}
