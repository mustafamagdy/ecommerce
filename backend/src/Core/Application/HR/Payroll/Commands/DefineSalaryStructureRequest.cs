using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class DefineSalaryStructureRequest : IRequest<Guid> // Returns SalaryStructure.Id
{
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public List<SalaryComponentDto> Earnings { get; set; } = new();
    public List<SalaryComponentDto> Deductions { get; set; } = new();
}
