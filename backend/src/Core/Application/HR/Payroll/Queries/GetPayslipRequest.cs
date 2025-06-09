using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetPayslipRequest : IRequest<PayslipDto>
{
    public Guid PayslipId { get; set; }

    public GetPayslipRequest(Guid payslipId) => PayslipId = payslipId;
}
