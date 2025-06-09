using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For LeaveType entity
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class GetLeaveTypeRequestHandler : IRequestHandler<GetLeaveTypeRequest, LeaveTypeDto>
{
    private readonly IReadRepository<LeaveType> _repository;
    private readonly IStringLocalizer<GetLeaveTypeRequestHandler> _t;

    public GetLeaveTypeRequestHandler(IReadRepository<LeaveType> repository, IStringLocalizer<GetLeaveTypeRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<LeaveTypeDto> Handle(GetLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var spec = new LeaveTypeByIdSpec(request.Id);
        var leaveTypeDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = leaveTypeDto ?? throw new NotFoundException(_t["LeaveType {0} Not Found.", request.Id]);

        return leaveTypeDto;
    }
}
