using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Leave entity
using MediatR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class GetPendingLeavesRequestHandler : IRequestHandler<GetPendingLeavesRequest, List<LeaveDto>>
{
    private readonly IReadRepository<Leave> _repository;

    public GetPendingLeavesRequestHandler(IReadRepository<Leave> repository)
    {
        _repository = repository;
    }

    public async Task<List<LeaveDto>> Handle(GetPendingLeavesRequest request, CancellationToken cancellationToken)
    {
        var spec = new PendingLeavesSpec();
        var leaves = await _repository.ListAsync(spec, cancellationToken);
        return leaves;
    }
}
