using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR.Attendance; // For AttendanceRecord entity
using FSH.WebApi.Application.HR.Attendance.Dtos;
using FSH.WebApi.Application.HR.Attendance.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Attendance.Queries;

public class GetAttendanceRecordRequestHandler : IRequestHandler<GetAttendanceRecordRequest, AttendanceRecordDto>
{
    private readonly IReadRepository<AttendanceRecord> _repository;
    private readonly IStringLocalizer<GetAttendanceRecordRequestHandler> _t;

    public GetAttendanceRecordRequestHandler(IReadRepository<AttendanceRecord> repository, IStringLocalizer<GetAttendanceRecordRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<AttendanceRecordDto> Handle(GetAttendanceRecordRequest request, CancellationToken cancellationToken)
    {
        var spec = new AttendanceRecordByEmployeeAndDateSpec(request.EmployeeId, request.Date);
        var recordDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = recordDto ?? throw new NotFoundException(_t["Attendance record not found for employee {0} on date {1}.", request.EmployeeId, request.Date.ToShortDateString()]);

        return recordDto;
    }
}
