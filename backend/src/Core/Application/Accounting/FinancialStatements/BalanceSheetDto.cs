using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public List<FinancialStatementLineDto> Assets { get; set; } = new();
    public decimal TotalAssets { get; set; }
    public List<FinancialStatementLineDto> Liabilities { get; set; } = new();
    public decimal TotalLiabilities { get; set; }
    public List<FinancialStatementLineDto> Equity { get; set; } = new();
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; } // Should equal TotalAssets
}
