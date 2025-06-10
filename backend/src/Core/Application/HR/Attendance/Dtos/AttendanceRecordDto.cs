namespace FSH.WebApi.Application.HR.Attendance.Dtos;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; } // e.g., "FirstName LastName"

    public DateTime Date { get; set; } // Date part only
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal? HoursWorked { get; set; } // In decimal hours
    public string? Notes { get; set; }
    public bool IsManuallyEntered { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
