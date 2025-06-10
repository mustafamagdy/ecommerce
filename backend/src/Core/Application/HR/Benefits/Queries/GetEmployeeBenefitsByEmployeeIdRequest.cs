using FSH.WebApi.Application.HR.Benefits.Dtos; // For EmployeeBenefitDto
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetEmployeeBenefitsByEmployeeIdRequest : IRequest<List<EmployeeBenefitDto>>
{
    public Guid EmployeeId { get; set; }

    public GetEmployeeBenefitsByEmployeeIdRequest(Guid employeeId) => EmployeeId = employeeId;
}
