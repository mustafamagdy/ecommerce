using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR.Attendance;
using FSH.WebApi.Application.HR.Attendance.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockOutRequestHandler : IRequestHandler<ClockOutRequest, Guid>
{
    private readonly IRepositoryWithEvents<AttendanceRecord> _attendanceRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public ClockOutRequestHandler(
        IRepositoryWithEvents<AttendanceRecord> attendanceRepo,
        ICurrentUser currentUser,
        IApplicationUnitOfWork uow,
        IStringLocalizer<ClockOutRequestHandler> localizer)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(ClockOutRequest request, CancellationToken cancellationToken)
    {
        Guid employeeId = request.EmployeeId ?? _currentUser.GetUserId();
        if (employeeId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(_t["User must be authenticated or EmployeeId provided."]);
        }

        DateTime today = request.ClockOutTime.Date; // Use the date part of ClockOutTime for record's Date

        var spec = new AttendanceRecordByEmployeeAndDateSpec(employeeId, today);
        var attendanceRecord = await _attendanceRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (attendanceRecord is null || !attendanceRecord.ClockInTime.HasValue)
        {
            throw new ConflictException(_t["Cannot clock out: No prior clock-in found for employee {0} today.", employeeId]);
        }

        if (attendanceRecord.ClockOutTime.HasValue)
        {
            throw new ConflictException(_t["Employee {0} has already clocked out today at {1}.", employeeId, attendanceRecord.ClockOutTime.Value]);
        }

        if (request.ClockOutTime < attendanceRecord.ClockInTime.Value)
        {
            throw new ValidationException(_t["Clock-out time cannot be earlier than clock-in time."]);
        }

        attendanceRecord.ClockOutTime = request.ClockOutTime;
        attendanceRecord.CalculateAndSetHoursWorked(); // Use the method from domain entity
        if (request.Notes is not null)
        {
            attendanceRecord.Notes = string.IsNullOrWhiteSpace(attendanceRecord.Notes)
                ? request.Notes
                : $"{attendanceRecord.Notes}\nClock-out: {request.Notes}"; // Append notes
        }

        attendanceRecord.AddDomainEvent(EntityUpdatedEvent.WithEntity(attendanceRecord));
        await _attendanceRepo.UpdateAsync(attendanceRecord, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return attendanceRecord.Id;
    }
}
