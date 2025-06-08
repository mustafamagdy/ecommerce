using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class GenerateBalanceSheetReportRequest : IRequest<Stream>
{
    [Required]
    public DateTime AsOfDate { get; set; }

    public GenerateBalanceSheetReportRequest(DateTime asOfDate)
    {
        AsOfDate = asOfDate;
    }
}
