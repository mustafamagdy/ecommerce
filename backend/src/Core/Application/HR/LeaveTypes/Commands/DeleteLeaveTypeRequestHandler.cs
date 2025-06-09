using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class DeleteLeaveTypeRequestHandler : IRequestHandler<DeleteLeaveTypeRequest, Guid>
{
    private readonly IRepositoryWithEvents<LeaveType> _leaveTypeRepository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;
    // private readonly IReadRepository<Leave> _leaveRepository; // To check if LeaveType is in use

    public DeleteLeaveTypeRequestHandler(
        IRepositoryWithEvents<LeaveType> leaveTypeRepository,
        IApplicationUnitOfWork uow,
        IStringLocalizer<DeleteLeaveTypeRequestHandler> localizer
        // IReadRepository<Leave> leaveRepository
        )
    {
        _leaveTypeRepository = leaveTypeRepository;
        _uow = uow;
        _t = localizer;
        // _leaveRepository = leaveRepository;
    }

    public async Task<Guid> Handle(DeleteLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        // TODO: Future enhancement: Add validation to check if any Leave entities are using this LeaveType.
        // This would require IReadRepository<Leave> and a specification like LeavesByLeaveTypeSpec.
        // Example:
        // var leavesUsingTypeSpec = new LeavesByLeaveTypeSpec(request.Id);
        // bool isLeaveTypeUsed = await _leaveReadRepository.AnyAsync(leavesUsingTypeSpec, cancellationToken);
        // if (isLeaveTypeUsed)
        // {
        //     throw new ConflictException(_t["LeaveType '{0}' cannot be deleted as it is currently used in leave requests.", request.Id]);
        // }

        var leaveType = await _leaveTypeRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leaveType ?? throw new NotFoundException(_t["LeaveType {0} Not Found.", request.Id]);

        leaveType.AddDomainEvent(EntityDeletedEvent.WithEntity(leaveType));

        await _leaveTypeRepository.DeleteAsync(leaveType, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return request.Id;
    }
}

// Placeholder for a specification if needed for validation.
// public class LeavesByLeaveTypeSpec : Specification<Leave>, ISingleResultSpecification
// {
//    public LeavesByLeaveTypeSpec(Guid leaveTypeId) => Query.Where(l => l.LeaveTypeId == leaveTypeId);
// }
