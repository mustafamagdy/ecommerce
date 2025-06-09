using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class GetLeaveRequest : IRequest<LeaveDto>
{
    public Guid Id { get; set; }

    public GetLeaveRequest(Guid id) => Id = id;
}
