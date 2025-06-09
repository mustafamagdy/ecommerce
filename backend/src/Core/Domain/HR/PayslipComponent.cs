namespace FSH.WebApi.Domain.HR;

public enum PayslipComponentType
{
    Earning,
    Deduction
}

public class PayslipComponent
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; } // Stores the calculated amount, not percentage or base for percentage
    public PayslipComponentType Type { get; set; }

    public PayslipComponent(string name, decimal amount, PayslipComponentType type)
    {
        Name = name;
        Amount = amount;
        Type = type;
    }

    // Parameterless constructor for EF Core
    public PayslipComponent() { }
}
