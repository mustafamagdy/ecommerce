using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

namespace FSH.WebApi.Domain.HR;

public class SalaryStructure : AuditableEntity
{
    public Guid EmployeeId { get; set; } // Foreign Key to Employee
    public virtual Employee? Employee { get; set; } // Navigation property

    public decimal BasicSalary { get; set; }

    public List<SalaryComponent> Earnings { get; set; } = new();
    public List<SalaryComponent> Deductions { get; set; } = new();

    // Method to calculate total earnings
    public decimal CalculateTotalEarnings()
    {
        return Earnings.Sum(c => c.CalculateActualValue(BasicSalary));
    }

    // Method to calculate total deductions
    public decimal CalculateTotalDeductions()
    {
        return Deductions.Sum(c => c.CalculateActualValue(BasicSalary));
    }

    // Method to calculate net salary
    public decimal CalculateNetSalary()
    {
        return BasicSalary + CalculateTotalEarnings() - CalculateTotalDeductions();
    }
}
