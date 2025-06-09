using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class CreateLeaveTypeRequest : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int DefaultBalance { get; set; }
}
