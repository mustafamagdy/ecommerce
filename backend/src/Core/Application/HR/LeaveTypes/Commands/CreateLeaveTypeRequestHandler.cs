using FSH.WebApi.Application.Common.Exceptions; // For ConflictException
using FSH.WebApi.Application.Common.Persistence; // For IRepositoryWithEvents, IApplicationUnitOfWork, IReadRepository
using FSH.WebApi.Domain.Common.Events; // For EntityCreatedEvent
using FSH.WebApi.Domain.HR; // For LeaveType entity
using FSH.WebApi.Application.HR.LeaveTypes.Queries; // For LeaveTypeByNameSpec
using MediatR; // For IRequestHandler
using Microsoft.Extensions.Localization; // For IStringLocalizer

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class CreateLeaveTypeRequestHandler : IRequestHandler<CreateLeaveTypeRequest, Guid>
{
    private readonly IRepositoryWithEvents<LeaveType> _leaveTypeRepository;
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public CreateLeaveTypeRequestHandler(
        IRepositoryWithEvents<LeaveType> leaveTypeRepository,
        IStringLocalizer<CreateLeaveTypeRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var spec = new LeaveTypeByNameSpec(request.Name);
        var existingLeaveType = await _leaveTypeRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (existingLeaveType is not null)
        {
            throw new ConflictException(_t["LeaveType with name '{0}' already exists.", request.Name]);
        }

        var leaveType = new LeaveType
        {
            Name = request.Name,
            DefaultBalance = request.DefaultBalance
            // Id, CreatedBy, CreatedOn etc. are handled by AuditableEntity
        };

        leaveType.AddDomainEvent(EntityCreatedEvent.WithEntity(leaveType));

        await _leaveTypeRepository.AddAsync(leaveType, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leaveType.Id;
    }
}
