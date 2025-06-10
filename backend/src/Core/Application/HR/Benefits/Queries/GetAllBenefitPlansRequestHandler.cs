using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;
using FSH.WebApi.Application.HR.Benefits.Specifications;
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetAllBenefitPlansRequestHandler : IRequestHandler<GetAllBenefitPlansRequest, List<BenefitPlanDto>>
{
    private readonly IReadRepository<BenefitPlan> _repository;

    public GetAllBenefitPlansRequestHandler(IReadRepository<BenefitPlan> repository)
    {
        _repository = repository;
    }

    public async Task<List<BenefitPlanDto>> Handle(GetAllBenefitPlansRequest request, CancellationToken cancellationToken)
    {
        var spec = new AllBenefitPlansSpec(request.IsActive);
        var plans = await _repository.ListAsync(spec, cancellationToken);
        return plans;
    }
}
