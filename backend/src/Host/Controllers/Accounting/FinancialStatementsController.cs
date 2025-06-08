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

    [HttpPost("financial-statements/profit-and-loss/report")]
    [OpenApiOperation("Generate a Profit and Loss PDF report for a specified period.", "")]
    public async Task<FileResult> GenerateProfitAndLossReportAsync(GenerateProfitAndLossReportRequest request)
    {
        var pdf = await _mediator.Send(request);
        var fileName = $"profit_loss_{request.FromDate:yyyyMMdd}_{request.ToDate:yyyyMMdd}.pdf";
        return File(pdf, "application/octet-stream", fileName);
    }

    [HttpPost("financial-statements/balance-sheet/report")]
    [OpenApiOperation("Generate a Balance Sheet PDF report as of a specified date.", "")]
    public async Task<FileResult> GenerateBalanceSheetReportAsync(GenerateBalanceSheetReportRequest request)
    {
        var pdf = await _mediator.Send(request);
        var fileName = $"balance_sheet_{request.AsOfDate:yyyyMMdd}.pdf";
        return File(pdf, "application/octet-stream", fileName);
    }
}
