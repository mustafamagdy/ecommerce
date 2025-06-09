namespace FSH.WebApi.Application.HR.Payroll;

public class SalaryComponentDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
}
