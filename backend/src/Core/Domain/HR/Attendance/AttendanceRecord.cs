using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

namespace FSH.WebApi.Domain.HR.Attendance;

public class AttendanceRecord : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    public DateTime Date { get; set; } // Date of the record (date part only)

    public DateTime? ClockInTime { get; set; }  // Full timestamp
    public DateTime? ClockOutTime { get; set; } // Full timestamp

    // Calculated property for HoursWorked
    public TimeSpan? HoursWorkedTimeSpan
    {
        get
        {
            if (ClockInTime.HasValue && ClockOutTime.HasValue && ClockOutTime.Value > ClockInTime.Value)
            {
                return ClockOutTime.Value - ClockInTime.Value;
            }
            return null;
        }
    }

    // Storing HoursWorked as decimal for easier aggregation, but can also use TimeSpan and convert.
    // This property might be better calculated on-the-fly or by a background service
    // rather than being a persisted field directly set, to avoid sync issues if ClockIn/Out changes.
    // For now, let's assume it might be set upon ClockOut or manual entry.
    public decimal? HoursWorked { get; set; }

    public string? Notes { get; set; } // e.g., reason for manual entry, forgot to clock out
    public bool IsManuallyEntered { get; set; } = false;

    // Constructor to ensure Date is set (date part only)
    public AttendanceRecord(Guid employeeId, DateTime date)
    {
        EmployeeId = employeeId;
        Date = date.Date; // Ensure only date part is stored
    }

    // Parameterless constructor for EF Core
    private AttendanceRecord() { }

    // Method to update HoursWorked (e.g. when clocking out or manually editing)
    public void CalculateAndSetHoursWorked()
    {
        if (ClockInTime.HasValue && ClockOutTime.HasValue && ClockOutTime.Value > ClockInTime.Value)
        {
            HoursWorked = (decimal)(ClockOutTime.Value - ClockInTime.Value).TotalHours;
        }
        else
        {
            HoursWorked = null;
        }
    }
}
