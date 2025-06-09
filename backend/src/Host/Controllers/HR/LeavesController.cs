using FSH.WebApi.Application.HR.Leaves.Commands;
using FSH.WebApi.Application.HR.Leaves.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;

namespace FSH.WebApi.Host.Controllers.HR;

public class LeavesController : VersionedApiController
{
    [HttpPost]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Create, FSHResource.HRLeaves)] (Employee applying for leave)
    [OpenApiOperation("Submit a new leave request.", "")]
    public async Task<ActionResult<Guid>> CreateAsync(CreateLeaveRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPut("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Update, FSHResource.HRLeavesSelf)] (Employee updating their own pending leave)
    [OpenApiOperation("Update an existing pending leave request.", "")]
    public async Task<ActionResult<Guid>> UpdateAsync(UpdateLeaveRequest request, Guid id)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("{id:guid}/cancel")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Cancel, FSHResource.HRLeavesSelf)] (Employee cancelling their own leave)
    [OpenApiOperation("Cancel a leave request.", "")]
    public async Task<ActionResult<Guid>> CancelAsync(Guid id)
    {
        return Ok(await Mediator.Send(new CancelLeaveRequest(id)));
    }

    [HttpPut("{id:guid}/status")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Approve, FSHResource.HRLeavesManager)] (Manager approving/rejecting leave)
    [OpenApiOperation("Update the status of a leave request (approve/reject).", "")]
    public async Task<ActionResult<Guid>> UpdateStatusAsync(UpdateLeaveStatusRequest request, Guid id)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpGet("{id:guid}")]
    // TODO: Add Permissions - e.g., View self or manager view
    [OpenApiOperation("Get leave request details by ID.", "")]
    public async Task<ActionResult<LeaveDto>> GetAsync(Guid id)
    {
        var leave = await Mediator.Send(new GetLeaveRequest(id));
        return Ok(leave);
    }

    [HttpGet("employee/{employeeId:guid}")]
    // TODO: Add Permissions - e.g., View self or manager view
    [OpenApiOperation("Get all leave requests for a specific employee.", "")]
    public async Task<ActionResult<List<LeaveDto>>> GetByEmployeeAsync(Guid employeeId)
    {
        var leaves = await Mediator.Send(new GetLeavesByEmployeeRequest(employeeId));
        return Ok(leaves);
    }

    [HttpGet("pending")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.ViewPending, FSHResource.HRLeavesManager)] (Manager viewing pending leaves)
    [OpenApiOperation("Get all pending leave requests.", "")]
    public async Task<ActionResult<List<LeaveDto>>> GetPendingAsync()
    {
        var leaves = await Mediator.Send(new GetPendingLeavesRequest());
        return Ok(leaves);
    }
}
