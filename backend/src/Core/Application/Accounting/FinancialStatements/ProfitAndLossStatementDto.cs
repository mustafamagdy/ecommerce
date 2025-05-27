using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class ProfitAndLossStatementDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<FinancialStatementLineDto> Revenue { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public List<FinancialStatementLineDto> CostOfGoodsSold { get; set; } = new(); // Assuming COGS is a subset of Expenses or handled separately
    public decimal TotalCostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public List<FinancialStatementLineDto> OperatingExpenses { get; set; } = new();
    public decimal TotalOperatingExpenses { get; set; }
    public decimal NetProfit { get; set; } // Or NetLoss
}
