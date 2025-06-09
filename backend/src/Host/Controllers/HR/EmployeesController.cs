using FSH.WebApi.Application.HR.Employees.Commands;
using FSH.WebApi.Application.HR.Employees.Queries;
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.HR;

public class EmployeesController : VersionedApiController
{
    [HttpPost]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Create, FSHResource.HREmployees)]
    [OpenApiOperation("Create a new employee.", "")]
    public async Task<ActionResult<Guid>> CreateAsync(CreateEmployeeRequest request)
    {
        return Ok(await Mediator.Send(request)); // Ok (200) is fine, or Created (201) if we return the DTO.
                                               // For Guid ID, Ok is common.
    }

    [HttpPut("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Update, FSHResource.HREmployees)]
    [OpenApiOperation("Update an existing employee.", "")]
    public async Task<ActionResult<Guid>> UpdateAsync(UpdateEmployeeRequest request, Guid id)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpDelete("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Delete, FSHResource.HREmployees)]
    [OpenApiOperation("Delete an employee.", "")]
    public async Task<ActionResult<Guid>> DeleteAsync(Guid id)
    {
        return Ok(await Mediator.Send(new DeleteEmployeeRequest(id)));
    }

    [HttpGet("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HREmployees)]
    [OpenApiOperation("Get employee details by ID.", "")]
    public async Task<ActionResult<EmployeeDto>> GetAsync(Guid id)
    {
        var employee = await Mediator.Send(new GetEmployeeRequest(id));
        return Ok(employee);
    }

    [HttpGet]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Search, FSHResource.HREmployees)]
    [OpenApiOperation("Search employees using available filters.", "")]
    public async Task<ActionResult<PaginationResponse<EmployeeDto>>> SearchAsync([FromQuery] SearchEmployeesRequest request)
    {
        // Note: SearchEmployeesRequest inherits PaginationFilter, so query params for pagination
        // like PageNumber, PageSize, OrderBy should be automatically bound from the query string.
        // Specific search params like Name, DepartmentId will also be bound from query.
        var employees = await Mediator.Send(request);
        return Ok(employees);
    }
}
