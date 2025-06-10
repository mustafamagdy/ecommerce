using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class SubmitTimesheetRequest : IRequest<Guid> // Assuming it might return Timesheet.Id or some confirmation
{
    public Guid? EmployeeId { get; set; } // Nullable if to be derived from ICurrentUser
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public string? Comments { get; set; }
}
