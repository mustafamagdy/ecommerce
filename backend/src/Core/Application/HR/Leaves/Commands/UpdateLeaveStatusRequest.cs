using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveStatusRequest : IRequest<Guid>
{
    public Guid Id { get; set; } // Leave Id
    public string Status { get; set; } = string.Empty; // "Approved" or "Rejected"
    public string? Notes { get; set; } // Manager's notes
}
