using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class GeneratePayslipRequest : IRequest<Guid> // Returns Payslip.Id
{
    public Guid EmployeeId { get; set; }
    public DateTime PayPeriodStartDate { get; set; }
    public DateTime PayPeriodEndDate { get; set; }
}
