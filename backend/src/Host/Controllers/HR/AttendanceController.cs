using FSH.WebApi.Application.HR.Attendance.Commands;
using FSH.WebApi.Application.HR.Attendance.Dtos;
using FSH.WebApi.Application.HR.Attendance.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;

namespace FSH.WebApi.Host.Controllers.HR;

public class AttendanceController : VersionedApiController
{
    // === Clock-In/Clock-Out Endpoints ===

    [HttpPost("clockin")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.ClockIn, FSHResource.HRAttendanceSelf)]
    [OpenApiOperation("Clock in for the current day or a specified time.", "")]
    public async Task<ActionResult<Guid>> ClockInAsync(ClockInRequest request)
    {
        // The ClockInRequest allows EmployeeId to be null,
        // in which case the handler is expected to use ICurrentUser.
        // If EmployeeId is provided, it might be an admin action.
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("clockout")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.ClockOut, FSHResource.HRAttendanceSelf)]
    [OpenApiOperation("Clock out for the current day or a specified time.", "")]
    public async Task<ActionResult<Guid>> ClockOutAsync(ClockOutRequest request)
    {
        // Similar to ClockInRequest, EmployeeId can be null.
        return Ok(await Mediator.Send(request));
    }

    // === Attendance Record Query Endpoints ===

    [HttpGet("record")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRAttendanceSelf)] or for manager
    [OpenApiOperation("Get attendance record for an employee on a specific date.", "")]
    public async Task<ActionResult<AttendanceRecordDto>> GetRecordAsync([FromQuery] Guid employeeId, [FromQuery] DateTime date)
    {
        if (employeeId == Guid.Empty || date == DateTime.MinValue)
        {
            return BadRequest("EmployeeId and Date are required query parameters.");
        }
        var record = await Mediator.Send(new GetAttendanceRecordRequest(employeeId, date));
        return Ok(record);
    }

    [HttpGet("records/period")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRAttendanceSelf)] or for manager
    [OpenApiOperation("Get attendance records for an employee over a period.", "")]
    public async Task<ActionResult<List<AttendanceRecordDto>>> GetRecordsForPeriodAsync(
        [FromQuery] Guid employeeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (employeeId == Guid.Empty || startDate == DateTime.MinValue || endDate == DateTime.MinValue)
        {
            return BadRequest("EmployeeId, StartDate, and EndDate are required query parameters.");
        }
        if (startDate > endDate)
        {
            return BadRequest("StartDate cannot be after EndDate.");
        }
        var records = await Mediator.Send(new GetAttendanceRecordsForPeriodRequest(employeeId, startDate, endDate));
        return Ok(records);
    }
}
