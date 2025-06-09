using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CreateLeaveRequest : IRequest<Guid>
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}
