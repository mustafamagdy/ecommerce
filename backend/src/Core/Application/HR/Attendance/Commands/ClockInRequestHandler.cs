using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR.Attendance;
using FSH.WebApi.Application.HR.Attendance.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockInRequestHandler : IRequestHandler<ClockInRequest, Guid>
{
    private readonly IRepositoryWithEvents<AttendanceRecord> _attendanceRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public ClockInRequestHandler(
        IRepositoryWithEvents<AttendanceRecord> attendanceRepo,
        ICurrentUser currentUser,
        IApplicationUnitOfWork uow,
        IStringLocalizer<ClockInRequestHandler> localizer)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(ClockInRequest request, CancellationToken cancellationToken)
    {
        Guid employeeId = request.EmployeeId ?? _currentUser.GetUserId(); // Assuming GetUserId returns EmployeeId
        if (employeeId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(_t["User must be authenticated or EmployeeId provided."]);
        }

        DateTime today = request.ClockInTime.Date; // Use the date part of ClockInTime for the record's Date

        var spec = new AttendanceRecordByEmployeeAndDateSpec(employeeId, today);
        var attendanceRecord = await _attendanceRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (attendanceRecord is not null)
        {
            if (attendanceRecord.ClockInTime.HasValue)
            {
                throw new ConflictException(_t["Employee {0} has already clocked in today at {1}.", employeeId, attendanceRecord.ClockInTime.Value]);
            }
            // If record exists but ClockInTime is null (e.g., manually created placeholder or error state)
            attendanceRecord.ClockInTime = request.ClockInTime;
            attendanceRecord.Notes = request.Notes ?? attendanceRecord.Notes; // Append or overwrite notes
            attendanceRecord.IsManuallyEntered = request.IsManuallyEntered || attendanceRecord.IsManuallyEntered; // If either is true
            attendanceRecord.AddDomainEvent(EntityUpdatedEvent.WithEntity(attendanceRecord));
            await _attendanceRepo.UpdateAsync(attendanceRecord, cancellationToken);
        }
        else
        {
            attendanceRecord = new AttendanceRecord(employeeId, today) // Constructor sets Date to today.Date
            {
                ClockInTime = request.ClockInTime,
                Notes = request.Notes,
                IsManuallyEntered = request.IsManuallyEntered
            };
            attendanceRecord.AddDomainEvent(EntityCreatedEvent.WithEntity(attendanceRecord));
            await _attendanceRepo.AddAsync(attendanceRecord, cancellationToken);
        }

        await _uow.CommitAsync(cancellationToken);
        return attendanceRecord.Id;
    }
}
