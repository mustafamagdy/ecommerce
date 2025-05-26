using FSH.WebApi.Application.Accounting;
using FSH.WebApi.Application.Accounting.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FSH.WebApi.Host.Controllers.Accounting;

[Route("api/v1/accounting/accounts")]
public class AccountsController : VersionedApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService) =>
        _accountService = accountService;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var accountId = await _accountService.CreateAccountAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAccountByIdAsync), new { id = accountId }, accountId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        return Ok(account);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> UpdateAccountAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var updatedAccount = await _accountService.UpdateAccountAsync(id, request, cancellationToken);
        return Ok(updatedAccount);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)] // Or Status204NoContent
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.DeleteAccountAsync(id, cancellationToken);
        return Ok(result); // Or return NoContent();
    }

    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccountDto>>> SearchAccountsAsync(SearchAccountsRequest request, CancellationToken cancellationToken)
    {
        var accounts = await _accountService.SearchAccountsAsync(request, cancellationToken);
        return Ok(accounts);
    }
}
