using FSH.WebApi.Domain.HR.Enums; // For TimesheetStatus

namespace FSH.WebApi.Application.HR.Attendance.Dtos;

public class TimesheetDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; } // e.g., "FirstName LastName"

    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal TotalHoursWorked { get; set; }

    public TimesheetStatus Status { get; set; }
    public string StatusDescription => Status.ToString();

    public DateTime? SubmittedDate { get; set; }
    public Guid? ApprovedByEmployeeId { get; set; } // Keep as Guid for consistency with entity
    public string? ApprovedByEmployeeName { get; set; } // To be populated
    public DateTime? ApprovedDate { get; set; }
    public string? Comments { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
