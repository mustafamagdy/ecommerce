using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CancelLeaveRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public CancelLeaveRequest(Guid id) => Id = id;
}
