using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class UpdateLeaveTypeRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int? DefaultBalance { get; set; }
}
