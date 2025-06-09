using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class GetLeavesByEmployeeRequest : IRequest<List<LeaveDto>> // Returns a list, pagination can be added later
{
    public Guid EmployeeId { get; set; }

    public GetLeavesByEmployeeRequest(Guid employeeId) => EmployeeId = employeeId;
}
