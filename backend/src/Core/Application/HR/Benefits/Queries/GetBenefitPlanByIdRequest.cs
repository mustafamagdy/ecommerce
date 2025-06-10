using FSH.WebApi.Application.HR.Benefits.Dtos; // For BenefitPlanDto
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetBenefitPlanByIdRequest : IRequest<BenefitPlanDto>
{
    public Guid Id { get; set; }

    public GetBenefitPlanByIdRequest(Guid id) => Id = id;
}
