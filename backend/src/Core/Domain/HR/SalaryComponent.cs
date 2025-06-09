namespace FSH.WebApi.Domain.HR;

public class SalaryComponent
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }

    // Constructor or factory method could be useful here
    public SalaryComponent(string name, decimal amount, bool isPercentage)
    {
        Name = name;
        Amount = amount;
        IsPercentage = isPercentage;
    }

    // Parameterless constructor for EF Core if it's treated as an entity or complex type
    public SalaryComponent() { }

    // Method to calculate actual value if it's a percentage of basic salary
    public decimal CalculateActualValue(decimal basicSalary)
    {
        return IsPercentage ? (basicSalary * Amount / 100) : Amount;
    }
}
