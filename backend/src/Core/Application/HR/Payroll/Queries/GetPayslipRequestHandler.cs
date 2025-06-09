using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Payslip entity
using FSH.WebApi.Application.HR.Payroll.Specifications; // For PayslipByIdSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetPayslipRequestHandler : IRequestHandler<GetPayslipRequest, PayslipDto>
{
    private readonly IReadRepository<Payslip> _payslipRepo;
    private readonly IStringLocalizer _t;

    public GetPayslipRequestHandler(
        IReadRepository<Payslip> payslipRepo,
        IStringLocalizer<GetPayslipRequestHandler> localizer)
    {
        _payslipRepo = payslipRepo;
        _t = localizer;
    }

    public async Task<PayslipDto> Handle(GetPayslipRequest request, CancellationToken cancellationToken)
    {
        var spec = new PayslipByIdSpec(request.PayslipId);
        var payslipDto = await _payslipRepo.FirstOrDefaultAsync(spec, cancellationToken);

        _ = payslipDto ?? throw new NotFoundException(_t["Payslip with ID {0} not found.", request.PayslipId]);

        return payslipDto;
    }
}
