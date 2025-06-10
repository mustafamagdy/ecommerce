using FSH.WebApi.Application.HR.Benefits.Commands;
using FSH.WebApi.Application.HR.Benefits.Dtos;
using FSH.WebApi.Application.HR.Benefits.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;

namespace FSH.WebApi.Host.Controllers.HR;

public class BenefitsController : VersionedApiController
{
    // === Benefit Plan Endpoints ===

    [HttpPost("plans")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Manage, FSHResource.HRBenefitPlans)]
    [OpenApiOperation("Define (create or update) a benefit plan.", "")]
    public async Task<ActionResult<Guid>> DefineBenefitPlanAsync(DefineBenefitPlanRequest request)
    {
        // DefineBenefitPlanRequestHandler handles create vs update based on request.Id presence
        return Ok(await Mediator.Send(request));
    }

    [HttpPut("plans/{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Manage, FSHResource.HRBenefitPlans)]
    [OpenApiOperation("Update an existing benefit plan.", "Ensure request body Id matches route Id.")]
    public async Task<ActionResult<Guid>> UpdateBenefitPlanAsync(Guid id, DefineBenefitPlanRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route ID and Request Body ID mismatch.");
        }
        // DefineBenefitPlanRequestHandler will fetch by request.Id and update
        return Ok(await Mediator.Send(request));
    }

    [HttpGet("plans/{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRBenefitPlans)]
    [OpenApiOperation("Get benefit plan details by ID.", "")]
    public async Task<ActionResult<BenefitPlanDto>> GetBenefitPlanByIdAsync(Guid id)
    {
        return Ok(await Mediator.Send(new GetBenefitPlanByIdRequest(id)));
    }

    [HttpGet("plans")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRBenefitPlans)]
    [OpenApiOperation("Get all benefit plans (optionally filter by active status).", "")]
    public async Task<ActionResult<List<BenefitPlanDto>>> GetAllBenefitPlansAsync([FromQuery] bool? isActive = true)
    {
        return Ok(await Mediator.Send(new GetAllBenefitPlansRequest(isActive)));
    }

    // === Employee Benefit Endpoints ===

    [HttpPost("employee-enrollments")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Enroll, FSHResource.HREmployeeBenefits)]
    [OpenApiOperation("Enroll an employee in a benefit plan.", "")]
    public async Task<ActionResult<Guid>> EnrollEmployeeInBenefitAsync(EnrollEmployeeInBenefitRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPut("employee-enrollments/{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Update, FSHResource.HREmployeeBenefits)]
    [OpenApiOperation("Update an employee's benefit enrollment details.", "Ensure request body Id matches route Id.")]
    public async Task<ActionResult<Guid>> UpdateEmployeeBenefitAsync(Guid id, UpdateEmployeeBenefitRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route ID and Request Body ID mismatch.");
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("employee-enrollments/{id:guid}/terminate")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Terminate, FSHResource.HREmployeeBenefits)]
    [OpenApiOperation("Terminate an employee's benefit enrollment.", "")]
    public async Task<ActionResult<Guid>> TerminateEmployeeBenefitAsync(Guid id, TerminateEmployeeBenefitRequest requestBody) // Renamed to avoid conflict
    {
        // Construct the actual request, ensuring EmployeeBenefitId comes from the route.
        var command = new TerminateEmployeeBenefitRequest
        {
            EmployeeBenefitId = id,
            TerminationDate = requestBody.TerminationDate,
            Reason = requestBody.Reason
        };
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("employee-enrollments/{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HREmployeeBenefits)]
    [OpenApiOperation("Get a specific employee benefit enrollment by its ID.", "")]
    public async Task<ActionResult<EmployeeBenefitDto>> GetEmployeeBenefitByIdAsync(Guid id)
    {
        return Ok(await Mediator.Send(new GetEmployeeBenefitByIdRequest(id)));
    }

    [HttpGet("employee/{employeeId:guid}/enrollments")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HREmployeeBenefits)] (Self or Manager)
    [OpenApiOperation("Get all benefit enrollments for a specific employee.", "")]
    public async Task<ActionResult<List<EmployeeBenefitDto>>> GetEmployeeBenefitsByEmployeeIdAsync(Guid employeeId)
    {
        return Ok(await Mediator.Send(new GetEmployeeBenefitsByEmployeeIdRequest(employeeId)));
    }
}
