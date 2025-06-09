using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CancelLeaveRequestHandler : IRequestHandler<CancelLeaveRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public CancelLeaveRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        ICurrentUser currentUser,
        IStringLocalizer<CancelLeaveRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        _currentUser = currentUser;
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(CancelLeaveRequest request, CancellationToken cancellationToken)
    {
        var leave = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leave ?? throw new NotFoundException(_t["Leave request with ID {0} not found.", request.Id]);

        Guid? currentEmployeeId = _currentUser.GetUserId(); // Assumes GetUserId() returns EmployeeId
        if (leave.EmployeeId != currentEmployeeId)
        {
            throw new ForbiddenException(_t["You are not authorized to cancel this leave request."]);
        }

        // Define cancellable states
        bool isCancellable = leave.Status == "Pending" ||
                             (leave.Status == "Approved" && leave.StartDate > DateTime.UtcNow); // Can cancel if approved but not yet started

        if (!isCancellable)
        {
            throw new ConflictException(_t["This leave request cannot be cancelled. It is already {0} or has started.", leave.Status]);
        }

        leave.Status = "Cancelled";

        await _leaveRepository.UpdateAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
