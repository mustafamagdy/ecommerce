using FSH.WebApi.Application.HR.Benefits.Dtos; // For EmployeeBenefitDto
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Queries;

public class GetEmployeeBenefitByIdRequest : IRequest<EmployeeBenefitDto>
{
    public Guid Id { get; set; } // EmployeeBenefitId

    public GetEmployeeBenefitByIdRequest(Guid id) => Id = id;
}
