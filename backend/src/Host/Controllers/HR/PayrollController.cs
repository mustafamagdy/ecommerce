using FSH.WebApi.Application.HR.Payroll; // Application services (Commands, Queries, DTOs)
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.HR;

public class PayrollController : VersionedApiController
{
    // === Salary Structure Endpoints ===

    [HttpPost("salary-structures")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Manage, FSHResource.PayrollSalaryStructures)]
    [OpenApiOperation("Define or update a salary structure for an employee.", "")]
    public async Task<ActionResult<Guid>> DefineSalaryStructureAsync(DefineSalaryStructureRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpGet("salary-structures/employee/{employeeId:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.PayrollSalaryStructures)]
    [OpenApiOperation("Get the salary structure for a specific employee.", "")]
    public async Task<ActionResult<SalaryStructureDto>> GetSalaryStructureAsync(Guid employeeId)
    {
        var salaryStructure = await Mediator.Send(new GetSalaryStructureRequest(employeeId));
        return Ok(salaryStructure);
    }

    // === Payslip Endpoints ===

    [HttpPost("payslips/generate")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Generate, FSHResource.PayrollPayslips)]
    [OpenApiOperation("Generate a new payslip for an employee for a given period.", "")]
    public async Task<ActionResult<Guid>> GeneratePayslipAsync(GeneratePayslipRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpGet("payslips/{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.PayrollPayslips)] (Self or Manager)
    [OpenApiOperation("Get payslip details by ID.", "")]
    public async Task<ActionResult<PayslipDto>> GetPayslipAsync(Guid id)
    {
        var payslip = await Mediator.Send(new GetPayslipRequest(id));
        return Ok(payslip);
    }

    [HttpGet("payslips/employee/{employeeId:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.PayrollPayslips)] (Self or Manager)
    [OpenApiOperation("Get payslips for a specific employee, with pagination.", "")]
    public async Task<ActionResult<PaginationResponse<PayslipDto>>> GetPayslipsByEmployeeAsync(Guid employeeId, [FromQuery] GetPayslipsByEmployeeRequest request)
    {
        if (employeeId != request.EmployeeId)
        {
            // Or handle this in a validator for GetPayslipsByEmployeeRequest if EmployeeId is part of route AND query
            // For now, ensuring consistency or prioritizing the route parameter.
            request.EmployeeId = employeeId;
        }
        var payslips = await Mediator.Send(request);
        return Ok(payslips);
    }
}
