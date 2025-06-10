using Ardalis.Specification;
using FSH.WebApi.Domain.HR.Attendance; // For AttendanceRecord entity
using FSH.WebApi.Domain.HR; // For Employee entity (to include for name)
using FSH.WebApi.Application.HR.Attendance.Dtos; // For AttendanceRecordDto

namespace FSH.WebApi.Application.HR.Attendance.Specifications;

public class AttendanceRecordByEmployeeAndDateSpec : Specification<AttendanceRecord, AttendanceRecordDto>, ISingleResultSpecification
{
    public AttendanceRecordByEmployeeAndDateSpec(Guid employeeId, DateTime date)
    {
        Query
            .Where(ar => ar.EmployeeId == employeeId && ar.Date == date.Date)
            .Include(ar => ar.Employee); // Include Employee for EmployeeName

        Query.Select(ar => new AttendanceRecordDto
        {
            Id = ar.Id,
            EmployeeId = ar.EmployeeId,
            EmployeeName = ar.Employee != null ? $"{ar.Employee.FirstName} {ar.Employee.LastName}" : null,
            Date = ar.Date,
            ClockInTime = ar.ClockInTime,
            ClockOutTime = ar.ClockOutTime,
            HoursWorked = ar.HoursWorked, // Assumes HoursWorked is correctly populated on entity
            Notes = ar.Notes,
            IsManuallyEntered = ar.IsManuallyEntered,
            CreatedOn = ar.CreatedOn,
            LastModifiedOn = ar.LastModifiedOn
        });
    }
}
