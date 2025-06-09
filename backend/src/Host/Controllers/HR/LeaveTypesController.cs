using FSH.WebApi.Application.HR.LeaveTypes.Commands;
using FSH.WebApi.Application.HR.LeaveTypes.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations; // For OpenApiOperation
using System.Collections.Generic; // For List

namespace FSH.WebApi.Host.Controllers.HR;

public class LeaveTypesController : VersionedApiController
{
    [HttpPost]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Create, FSHResource.HRLeaveTypes)]
    [OpenApiOperation("Create a new leave type.", "")]
    public async Task<ActionResult<Guid>> CreateAsync(CreateLeaveTypeRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPut("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Update, FSHResource.HRLeaveTypes)]
    [OpenApiOperation("Update an existing leave type.", "")]
    public async Task<ActionResult<Guid>> UpdateAsync(UpdateLeaveTypeRequest request, Guid id)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpDelete("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Delete, FSHResource.HRLeaveTypes)]
    [OpenApiOperation("Delete a leave type.", "")]
    public async Task<ActionResult<Guid>> DeleteAsync(Guid id)
    {
        return Ok(await Mediator.Send(new DeleteLeaveTypeRequest(id)));
    }

    [HttpGet("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRLeaveTypes)]
    [OpenApiOperation("Get leave type details by ID.", "")]
    public async Task<ActionResult<LeaveTypeDto>> GetAsync(Guid id)
    {
        var leaveType = await Mediator.Send(new GetLeaveTypeRequest(id));
        return Ok(leaveType);
    }

    [HttpGet]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRLeaveTypes)] // Or Search if more specific
    [OpenApiOperation("Get all leave types.", "")]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetAllAsync()
    {
        // GetAllLeaveTypesRequest might take pagination params in future.
        // For now, it takes no parameters.
        var leaveTypes = await Mediator.Send(new GetAllLeaveTypesRequest());
        return Ok(leaveTypes);
    }
}
