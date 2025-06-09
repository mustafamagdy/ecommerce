using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveRequest : IRequest<Guid>
{
    public Guid Id { get; set; } // Leave Id to update
    // public Guid EmployeeId { get; set; } // Usually not changed, or taken from context

    public Guid? LeaveTypeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
