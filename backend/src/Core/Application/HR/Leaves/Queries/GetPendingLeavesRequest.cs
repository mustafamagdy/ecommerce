using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

// Returns a list, pagination can be added later if needed for manager views.
public class GetPendingLeavesRequest : IRequest<List<LeaveDto>>
{
}
