using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Attendance;
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Application.HR.Attendance.Dtos;

namespace FSH.WebApi.Application.HR.Attendance.Specifications;

public class AttendanceRecordsByEmployeeAndPeriodSpec : Specification<AttendanceRecord, AttendanceRecordDto>
{
    public AttendanceRecordsByEmployeeAndPeriodSpec(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        Query
            .Where(ar => ar.EmployeeId == employeeId && ar.Date >= startDate.Date && ar.Date <= endDate.Date)
            .Include(ar => ar.Employee) // Include Employee for EmployeeName
            .OrderBy(ar => ar.Date);    // Order by date

        Query.Select(ar => new AttendanceRecordDto
        {
            Id = ar.Id,
            EmployeeId = ar.EmployeeId,
            EmployeeName = ar.Employee != null ? $"{ar.Employee.FirstName} {ar.Employee.LastName}" : null,
            Date = ar.Date,
            ClockInTime = ar.ClockInTime,
            ClockOutTime = ar.ClockOutTime,
            HoursWorked = ar.HoursWorked,
            Notes = ar.Notes,
            IsManuallyEntered = ar.IsManuallyEntered,
            CreatedOn = ar.CreatedOn,
            LastModifiedOn = ar.LastModifiedOn
        });
    }
}
