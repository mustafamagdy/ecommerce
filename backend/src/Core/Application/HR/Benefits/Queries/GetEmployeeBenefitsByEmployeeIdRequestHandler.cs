using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Dtos;
using FSH.WebApi.Application.HR.Benefits.Specifications;
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetEmployeeBenefitsByEmployeeIdRequestHandler : IRequestHandler<GetEmployeeBenefitsByEmployeeIdRequest, List<EmployeeBenefitDto>>
{
    private readonly IReadRepository<EmployeeBenefit> _repository;

    public GetEmployeeBenefitsByEmployeeIdRequestHandler(IReadRepository<EmployeeBenefit> repository)
    {
        _repository = repository;
    }

    public async Task<List<EmployeeBenefitDto>> Handle(GetEmployeeBenefitsByEmployeeIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new EmployeeBenefitsByEmployeeIdSpec(request.EmployeeId);
        // Ensure that the spec correctly projects EmployeeBenefit to EmployeeBenefitDto
        var benefits = await _repository.ListAsync(spec, cancellationToken);
        return benefits;
    }
}
