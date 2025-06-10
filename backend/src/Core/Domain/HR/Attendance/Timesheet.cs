using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.HR.Enums; // For TimesheetStatus

namespace FSH.WebApi.Domain.HR.Attendance;

public class Timesheet : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    public DateTime PeriodStartDate { get; set; } // Date part
    public DateTime PeriodEndDate { get; set; }   // Date part

    public decimal TotalHoursWorked { get; set; } // Sum of AttendanceRecord.HoursWorked for the period
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Open;

    public DateTime? SubmittedDate { get; set; }

    public Guid? ApprovedByEmployeeId { get; set; } // Changed name for clarity, FK to Employee
    public virtual Employee? Approver { get; set; }   // Navigation property for the approver

    public DateTime? ApprovedDate { get; set; }
    public string? Comments { get; set; } // For employee or approver

    // Optional: List of related attendance records.
    // Deciding against including it directly to avoid loading large datasets by default.
    // Can be queried separately if needed: e.g., GetAttendanceRecordsForTimesheet(timesheetId).
    // public List<AttendanceRecord> AttendanceRecords { get; private set; } = new();

    public Timesheet(Guid employeeId, DateTime periodStartDate, DateTime periodEndDate)
    {
        EmployeeId = employeeId;
        PeriodStartDate = periodStartDate.Date;
        PeriodEndDate = periodEndDate.Date;
        Status = TimesheetStatus.Open;
    }

    // Parameterless constructor for EF Core
    private Timesheet() { }
}
