using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For LeaveType entity
using MediatR;

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class GetAllLeaveTypesRequestHandler : IRequestHandler<GetAllLeaveTypesRequest, List<LeaveTypeDto>>
{
    private readonly IReadRepository<LeaveType> _repository;

    public GetAllLeaveTypesRequestHandler(IReadRepository<LeaveType> repository)
    {
        _repository = repository;
    }

    public async Task<List<LeaveTypeDto>> Handle(GetAllLeaveTypesRequest request, CancellationToken cancellationToken)
    {
        var spec = new AllLeaveTypesSpec();
        var leaveTypes = await _repository.ListAsync(spec, cancellationToken);
        return leaveTypes;
    }
}
