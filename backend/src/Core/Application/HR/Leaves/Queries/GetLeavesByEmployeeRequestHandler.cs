using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Leave entity
using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class GetLeavesByEmployeeRequestHandler : IRequestHandler<GetLeavesByEmployeeRequest, List<LeaveDto>>
{
    private readonly IReadRepository<Leave> _repository;

    public GetLeavesByEmployeeRequestHandler(IReadRepository<Leave> repository)
    {
        _repository = repository;
    }

    public async Task<List<LeaveDto>> Handle(GetLeavesByEmployeeRequest request, CancellationToken cancellationToken)
    {
        var spec = new LeavesByEmployeeSpec(request.EmployeeId);
        var leaves = await _repository.ListAsync(spec, cancellationToken);
        return leaves;
    }
}
