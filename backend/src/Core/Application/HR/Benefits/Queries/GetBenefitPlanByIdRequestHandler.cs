using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;
using FSH.WebApi.Application.HR.Benefits.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetBenefitPlanByIdRequestHandler : IRequestHandler<GetBenefitPlanByIdRequest, BenefitPlanDto>
{
    private readonly IReadRepository<BenefitPlan> _repository;
    private readonly IStringLocalizer<GetBenefitPlanByIdRequestHandler> _t;

    public GetBenefitPlanByIdRequestHandler(IReadRepository<BenefitPlan> repository, IStringLocalizer<GetBenefitPlanByIdRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<BenefitPlanDto> Handle(GetBenefitPlanByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new BenefitPlanByIdSpec(request.Id);
        var planDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = planDto ?? throw new NotFoundException(_t["Benefit Plan with ID {0} Not Found.", request.Id]);

        return planDto;
    }
}
