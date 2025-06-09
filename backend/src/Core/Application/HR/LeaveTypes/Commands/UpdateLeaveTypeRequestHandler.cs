using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.LeaveTypes.Queries; // For LeaveTypeByNameSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class UpdateLeaveTypeRequestHandler : IRequestHandler<UpdateLeaveTypeRequest, Guid>
{
    private readonly IRepositoryWithEvents<LeaveType> _leaveTypeRepository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateLeaveTypeRequestHandler(
        IRepositoryWithEvents<LeaveType> leaveTypeRepository,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateLeaveTypeRequestHandler> localizer)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leaveType ?? throw new NotFoundException(_t["LeaveType {0} Not Found.", request.Id]);

        // Check for name uniqueness if name is being changed
        if (request.Name is not null && request.Name != leaveType.Name)
        {
            var spec = new LeaveTypeByNameSpec(request.Name);
            var existingLeaveType = await _leaveTypeRepository.FirstOrDefaultAsync(spec, cancellationToken);
            if (existingLeaveType is not null && existingLeaveType.Id != leaveType.Id)
            {
                throw new ConflictException(_t["LeaveType with name '{0}' already exists.", request.Name]);
            }
            leaveType.Name = request.Name;
        }

        if (request.DefaultBalance.HasValue)
        {
            leaveType.DefaultBalance = request.DefaultBalance.Value;
        }

        // IRepositoryWithEvents handles EntityUpdatedEvent implicitly
        await _leaveTypeRepository.UpdateAsync(leaveType, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return request.Id;
    }
}
