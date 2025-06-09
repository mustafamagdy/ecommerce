using MediatR;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class MarkPayslipAsPaidRequest : IRequest<Guid> // Returns PayslipId
{
    public Guid PayslipId { get; set; }

    public MarkPayslipAsPaidRequest(Guid payslipId)
    {
        PayslipId = payslipId;
    }
}
