using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser (potentially for manager checks)
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveStatusRequestHandler : IRequestHandler<UpdateLeaveStatusRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    // private readonly ICurrentUser _currentUser; // Potentially for manager authorization
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public UpdateLeaveStatusRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        // ICurrentUser currentUser,
        IStringLocalizer<UpdateLeaveStatusRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        // _currentUser = currentUser;
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(UpdateLeaveStatusRequest request, CancellationToken cancellationToken)
    {
        var leave = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leave ?? throw new NotFoundException(_t["Leave request with ID {0} not found.", request.Id]);

        // TODO: Authorization: Ensure the current user is a manager of the employee who requested the leave.
        // This would involve fetching leave.Employee.ManagerId and comparing with _currentUser.GetEmployeeId().

        if (leave.Status != "Pending")
        {
            throw new ConflictException(_t["Only pending leave requests can be approved or rejected. This leave is already {0}.", leave.Status]);
        }

        if (request.Status != "Approved" && request.Status != "Rejected")
        {
            throw new ValidationException(_t["Invalid status value. Must be 'Approved' or 'Rejected'."]);
        }

        leave.Status = request.Status;
        leave.Notes = request.Notes ?? leave.Notes; // Update notes if provided

        // TODO: If "Approved", deduct from leave balance (complex, follow-up enhancement).
        // This involves:
        // 1. Calculating leave duration.
        // 2. Accessing employee's leave balance for this LeaveType.
        // 3. Updating the balance. This might be in a separate table or service.

        await _leaveRepository.UpdateAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
