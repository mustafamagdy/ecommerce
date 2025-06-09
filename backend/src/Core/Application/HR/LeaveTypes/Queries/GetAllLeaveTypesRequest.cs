using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

// Using List<LeaveTypeDto> as the response type.
// Could be changed to PaginationResponse<LeaveTypeDto> if pagination is added later.
public class GetAllLeaveTypesRequest : IRequest<List<LeaveTypeDto>>
{
}
