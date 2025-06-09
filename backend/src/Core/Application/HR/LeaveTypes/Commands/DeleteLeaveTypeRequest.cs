using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class DeleteLeaveTypeRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteLeaveTypeRequest(Guid id) => Id = id;
}
