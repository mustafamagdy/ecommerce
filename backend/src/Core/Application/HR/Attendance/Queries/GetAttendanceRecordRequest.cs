using FSH.WebApi.Application.HR.Attendance.Dtos; // For AttendanceRecordDto
using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Queries;

public class GetAttendanceRecordRequest : IRequest<AttendanceRecordDto>
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }

    public GetAttendanceRecordRequest(Guid employeeId, DateTime date)
    {
        EmployeeId = employeeId;
        Date = date;
    }
}
