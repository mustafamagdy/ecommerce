namespace FSH.WebApi.Application.HR.Payroll;

// Reusing the domain enum here, or could define a separate DTO enum
using FSH.WebApi.Domain.HR; // For PayslipComponentType

public class PayslipComponentDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PayslipComponentType Type { get; set; }
}
