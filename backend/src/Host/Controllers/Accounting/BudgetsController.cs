using FSH.WebApi.Application.Accounting.Budgets;
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.Accounting;

public class BudgetsController : VersionedApiController
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("budgets")]
    [OpenApiOperation("Create a new budget.", "")]
    // [MustHavePermission(FSHAction.Create, FSHResource.Budgets)]
    public async Task<ActionResult<Guid>> CreateBudgetAsync(CreateBudgetRequest request)
    {
        var budgetId = await _mediator.Send(request);
        return Ok(budgetId);
    }

    [HttpPut("budgets/{id:guid}")]
    [OpenApiOperation("Update an existing budget.", "")]
    // [MustHavePermission(FSHAction.Update, FSHResource.Budgets)]
    public async Task<ActionResult<Guid>> UpdateBudgetAsync(Guid id, UpdateBudgetRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route ID does not match request ID.");
        }
        var budgetId = await _mediator.Send(request);
        return Ok(budgetId);
    }

    [HttpGet("budgets/{id:guid}")]
    [OpenApiOperation("Get budget details by ID.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.Budgets)]
    public async Task<ActionResult<BudgetDto>> GetBudgetByIdAsync(Guid id)
    {
        var budgetDto = await _mediator.Send(new GetBudgetRequest(id));
        return Ok(budgetDto);
    }

    [HttpPost("budgets/search")]
    [OpenApiOperation("Search budgets using available filters.", "")]
    // [MustHavePermission(FSHAction.Search, FSHResource.Budgets)]
    public async Task<ActionResult<PaginationResponse<BudgetDto>>> SearchBudgetsAsync(SearchBudgetsRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("budgets/{id:guid}")]
    [OpenApiOperation("Delete a budget.", "")]
    // [MustHavePermission(FSHAction.Delete, FSHResource.Budgets)]
    public async Task<ActionResult<Guid>> DeleteBudgetAsync(Guid id)
    {
        var budgetId = await _mediator.Send(new DeleteBudgetRequest(id));
        return Ok(budgetId);
    }
}
