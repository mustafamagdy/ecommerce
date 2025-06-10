using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockOutRequest : IRequest<Guid> // Returns AttendanceRecord.Id
{
    public Guid? EmployeeId { get; set; } // Nullable if to be derived from ICurrentUser
    public DateTime ClockOutTime { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    // IsManuallyEntered might be relevant if a manager is clocking someone out.
    // For now, assume employee is clocking themselves out, so not manually entered in this context.
}
