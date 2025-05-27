using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class GenerateProfitAndLossRequest : IRequest<ProfitAndLossStatementDto>
{
    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    public GenerateProfitAndLossRequest(DateTime fromDate, DateTime toDate)
    {
        FromDate = fromDate;
        ToDate = toDate;
    }
}
