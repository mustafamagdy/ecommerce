using MediatR;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class CancelPayslipRequest : IRequest<Guid> // Returns PayslipId
{
    public Guid PayslipId { get; set; }
    public string? Reason { get; set; } // Optional cancellation reason

    public CancelPayslipRequest(Guid payslipId, string? reason = null)
    {
        PayslipId = payslipId;
        Reason = reason;
    }
}
