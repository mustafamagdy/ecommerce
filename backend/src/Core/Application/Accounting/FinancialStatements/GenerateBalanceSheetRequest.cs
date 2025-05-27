using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class GenerateBalanceSheetRequest : IRequest<BalanceSheetDto>
{
    [Required]
    public DateTime AsOfDate { get; set; }

    public GenerateBalanceSheetRequest(DateTime asOfDate)
    {
        AsOfDate = asOfDate;
    }
}
