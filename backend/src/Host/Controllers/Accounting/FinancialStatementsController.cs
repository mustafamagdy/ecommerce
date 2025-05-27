using FSH.WebApi.Application.Accounting.FinancialStatements;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using NSwag.Annotations; // For OpenApiOperation

namespace FSH.WebApi.Host.Controllers.Accounting;

public class FinancialStatementsController : VersionedApiController
{
    private readonly IMediator _mediator;

    public FinancialStatementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("financial-statements/profit-and-loss")]
    [OpenApiOperation("Generate a Profit and Loss statement for a specified period.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.FinancialStatements)] // Example permission
    public async Task<ActionResult<ProfitAndLossStatementDto>> GenerateProfitAndLossAsync(GenerateProfitAndLossRequest request)
    {
        var statementDto = await _mediator.Send(request);
        return Ok(statementDto);
    }

    [HttpPost("financial-statements/balance-sheet")]
    [OpenApiOperation("Generate a Balance Sheet statement as of a specified date.", "")]
    // [MustHavePermission(FSHAction.View, FSHResource.FinancialStatements)] // Example permission
    public async Task<ActionResult<BalanceSheetDto>> GenerateBalanceSheetAsync(GenerateBalanceSheetRequest request)
    {
        var statementDto = await _mediator.Send(request);
        return Ok(statementDto);
    }
}
