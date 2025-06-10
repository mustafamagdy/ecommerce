using FSH.WebApi.Application.HR.Attendance.Dtos; // For AttendanceRecordDto
using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Queries;

public class GetAttendanceRecordsForPeriodRequest : IRequest<List<AttendanceRecordDto>>
{
    public Guid EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetAttendanceRecordsForPeriodRequest(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }
}
