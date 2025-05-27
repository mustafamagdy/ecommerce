using FSH.WebApi.Application.Accounting.Accounts;
using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Required for AllowAnonymous or specific permissions
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.Accounting;

// The route would typically be "api/v{version:apiVersion}/accounting"
// However, individual methods will specify "accounts" part of the path.
// Or, we can add [Route("api/v{version:apiVersion}/accounting/accounts")] to the controller
// and then methods will have simpler routes like [HttpPost], [HttpGet("{id:guid}")].
// For now, let's assume "api/v{version:apiVersion}/accounting" is the base for this controller
// and "accounts" will be part of each method's route attribute.

public class AccountsController : VersionedApiController // Changed name to AccountsController for clarity as it handles Accounts
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("accounts")] // Route: api/v1/accounting/accounts
    [OpenApiOperation("Create a new account.", "")]
    // [MustHavePermission(FSHAction.Create, FSHResource.Accounts)] // Example permission
    public async Task<ActionResult<Guid>> CreateAccountAsync(CreateAccountRequest request)
    {
        var accountId = await _mediator.Send(request);
        return Ok(accountId);
    }

    [HttpPut("accounts/{id:guid}")] // Route: api/v1/accounting/accounts/{id}
    [OpenApiOperation("Update an existing account.", "")]
    // [MustHavePermission(FSHAction.Update, FSHResource.Accounts)] // Example permission
    public async Task<ActionResult<Guid>> UpdateAccountAsync(Guid id, UpdateAccountRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route ID does not match request ID.");
        }
        var accountId = await _mediator.Send(request);
        return Ok(accountId);
    }

    [HttpGet("accounts/{id:guid}")] // Route: api/v1/accounting/accounts/{id}
    [OpenApiOperation("Get account details by ID.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.Accounts)] // Example permission
    public async Task<ActionResult<AccountDto>> GetAccountByIdAsync(Guid id)
    {
        var accountDto = await _mediator.Send(new GetAccountRequest(id));
        return Ok(accountDto);
    }

    [HttpPost("accounts/search")] // Route: api/v1/accounting/accounts/search
    [OpenApiOperation("Search accounts using available filters.", "")]
    // [MustHavePermission(FSHAction.Search, FSHResource.Accounts)] // Example permission
    public async Task<ActionResult<PaginationResponse<AccountDto>>> SearchAccountsAsync(SearchAccountsRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    // Optional: Delete endpoint would go here if DeleteAccountRequest and handler were defined.
    // [HttpDelete("accounts/{id:guid}")]
    // [OpenApiOperation("Delete an account.", "")]
    // // [MustHavePermission(FSHAction.Delete, FSHResource.Accounts)]
    // public async Task<ActionResult<Guid>> DeleteAccountAsync(Guid id)
    // {
    //     var accountId = await _mediator.Send(new DeleteAccountRequest(id)); // Assuming DeleteAccountRequest exists
    //     return Ok(accountId);
    // }
}
