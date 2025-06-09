using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetSalaryStructureRequest : IRequest<SalaryStructureDto>
{
    public Guid EmployeeId { get; set; }

    public GetSalaryStructureRequest(Guid employeeId) => EmployeeId = employeeId;
}
