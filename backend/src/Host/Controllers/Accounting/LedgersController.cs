using FSH.WebApi.Application.Accounting.Ledgers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.Accounting;

public class LedgersController : VersionedApiController
{
    private readonly IMediator _mediator;

    public LedgersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ledgers/account-statement")]
    [OpenApiOperation("Get account ledger (statement) for a specific account and period.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.Ledgers)] // Example permission
    public async Task<ActionResult<AccountLedgerDto>> GetAccountLedgerAsync(GetAccountLedgerRequest request)
    {
        // GetAccountLedgerRequest already includes AccountId, FromDate, ToDate
        var accountLedgerDto = await _mediator.Send(request);
        return Ok(accountLedgerDto);
    }
}
