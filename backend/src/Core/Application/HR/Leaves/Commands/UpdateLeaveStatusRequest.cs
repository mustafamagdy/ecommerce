using MediatR;

using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveStatusRequest : IRequest<Guid>
{
    public Guid Id { get; set; } // Leave Id
    public LeaveStatusEnum Status { get; set; } // Changed to Enum
    public string? Notes { get; set; } // Manager's notes
}
