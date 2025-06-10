using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;
using FSH.WebApi.Application.HR.Benefits.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetEmployeeBenefitByIdRequestHandler : IRequestHandler<GetEmployeeBenefitByIdRequest, EmployeeBenefitDto>
{
    private readonly IReadRepository<EmployeeBenefit> _repository;
    private readonly IStringLocalizer<GetEmployeeBenefitByIdRequestHandler> _t;

    public GetEmployeeBenefitByIdRequestHandler(IReadRepository<EmployeeBenefit> repository, IStringLocalizer<GetEmployeeBenefitByIdRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<EmployeeBenefitDto> Handle(GetEmployeeBenefitByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new EmployeeBenefitByIdSpec(request.Id);
        var benefitDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = benefitDto ?? throw new NotFoundException(_t["Employee Benefit enrollment with ID {0} Not Found.", request.Id]);

        return benefitDto;
    }
}
