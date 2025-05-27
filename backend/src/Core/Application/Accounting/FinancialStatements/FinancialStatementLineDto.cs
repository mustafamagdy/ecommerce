namespace FSH.WebApi.Application.Accounting.FinancialStatements;

public class FinancialStatementLineDto
{
    public string AccountName { get; set; } = default!;
    public string? AccountNumber { get; set; }
    public decimal Amount { get; set; }
    public bool IsTotal { get; set; } = false;
}
