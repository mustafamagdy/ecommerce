using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Attendance; // For AttendanceRecord entity
using FSH.WebApi.Application.HR.Attendance.Dtos;
using FSH.WebApi.Application.HR.Attendance.Specifications;
using MediatR;

namespace FSH.WebApi.Application.HR.Attendance.Queries;

public class GetAttendanceRecordsForPeriodRequestHandler : IRequestHandler<GetAttendanceRecordsForPeriodRequest, List<AttendanceRecordDto>>
{
    private readonly IReadRepository<AttendanceRecord> _repository;

    public GetAttendanceRecordsForPeriodRequestHandler(IReadRepository<AttendanceRecord> repository)
    {
        _repository = repository;
    }

    public async Task<List<AttendanceRecordDto>> Handle(GetAttendanceRecordsForPeriodRequest request, CancellationToken cancellationToken)
    {
        var spec = new AttendanceRecordsByEmployeeAndPeriodSpec(request.EmployeeId, request.StartDate, request.EndDate);
        var records = await _repository.ListAsync(spec, cancellationToken);
        return records; // Assuming ListAsync returns List<AttendanceRecordDto> due to spec projection
    }
}
