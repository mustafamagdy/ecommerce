using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class GetLeaveTypeRequest : IRequest<LeaveTypeDto>
{
    public Guid Id { get; set; }

    public GetLeaveTypeRequest(Guid id) => Id = id;
}
