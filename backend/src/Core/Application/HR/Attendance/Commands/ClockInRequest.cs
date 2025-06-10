using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockInRequest : IRequest<Guid> // Returns AttendanceRecord.Id
{
    public Guid? EmployeeId { get; set; } // Nullable if to be derived from ICurrentUser
    public DateTime ClockInTime { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool IsManuallyEntered { get; set; } = false; // Default to not manual
}
