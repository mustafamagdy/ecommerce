using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Leave entity
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class GetLeaveRequestHandler : IRequestHandler<GetLeaveRequest, LeaveDto>
{
    private readonly IReadRepository<Leave> _repository;
    private readonly IStringLocalizer<GetLeaveRequestHandler> _t;

    public GetLeaveRequestHandler(IReadRepository<Leave> repository, IStringLocalizer<GetLeaveRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<LeaveDto> Handle(GetLeaveRequest request, CancellationToken cancellationToken)
    {
        var spec = new LeaveByIdSpec(request.Id);
        var leaveDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = leaveDto ?? throw new NotFoundException(_t["Leave Request {0} Not Found.", request.Id]);

        return leaveDto;
    }
}
