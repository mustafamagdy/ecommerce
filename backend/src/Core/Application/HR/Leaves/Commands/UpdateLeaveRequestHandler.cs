using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum
using FSH.WebApi.Application.HR.Leaves.Specifications; // For OverlappingLeaveSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveRequestHandler : IRequestHandler<UpdateLeaveRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    private readonly ICurrentUser _currentUser; // To check ownership
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public UpdateLeaveRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        ICurrentUser currentUser,
        IStringLocalizer<UpdateLeaveRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        _currentUser = currentUser;
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(UpdateLeaveRequest request, CancellationToken cancellationToken)
    {
        var leave = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leave ?? throw new NotFoundException(_t["Leave request with ID {0} not found.", request.Id]);

        // Authorization: Check if the current user owns this leave request.
        // The EmployeeId on the leave should match the current user's Employee ID.
        // This assumes CurrentUser.GetEmployeeId() or similar exists and works.
        // For this example, let's assume _currentUser.GetUserId() returns the EmployeeId.
        Guid? currentEmployeeId = _currentUser.GetUserId(); // Or a more specific GetEmployeeId()
        if (leave.EmployeeId != currentEmployeeId)
        {
            throw new ForbiddenException(_t["You are not authorized to update this leave request."]);
        }

        if (leave.Status != LeaveStatusEnum.Pending) // Using Enum
        {
            throw new ConflictException(_t["Only pending leave requests can be updated. This leave is already {0}.", leave.Status]);
        }

        // Update fields if provided
        bool datesChanged = false;
        if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != leave.LeaveTypeId)
        {
            leave.LeaveTypeId = request.LeaveTypeId.Value;
        }
        if (request.StartDate.HasValue && request.StartDate.Value != leave.StartDate)
        {
            leave.StartDate = request.StartDate.Value;
            datesChanged = true;
        }
        if (request.EndDate.HasValue && request.EndDate.Value != leave.EndDate)
        {
            leave.EndDate = request.EndDate.Value;
            datesChanged = true;
        }
        if (request.Reason is not null && request.Reason != leave.Reason)
        {
            leave.Reason = request.Reason;
        }

        // If dates changed, check for overlaps again
        if (datesChanged)
        {
            var overlappingSpec = new OverlappingLeaveSpec(leave.EmployeeId, leave.StartDate, leave.EndDate, leave.Id);
            var existingOverlappingLeave = await _leaveRepository.FirstOrDefaultAsync(overlappingSpec, cancellationToken);
            if (existingOverlappingLeave is not null)
            {
                throw new ConflictException(_t["An overlapping leave request already exists for the new dates."]);
            }
        }

        // IRepositoryWithEvents handles EntityUpdatedEvent
        await _leaveRepository.UpdateAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
