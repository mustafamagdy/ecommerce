using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Payslip, PayslipStatus
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class MarkPayslipAsPaidRequestHandler : IRequestHandler<MarkPayslipAsPaidRequest, Guid>
{
    private readonly IRepositoryWithEvents<Payslip> _payslipRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public MarkPayslipAsPaidRequestHandler(
        IRepositoryWithEvents<Payslip> payslipRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<MarkPayslipAsPaidRequestHandler> localizer)
    {
        _payslipRepo = payslipRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(MarkPayslipAsPaidRequest request, CancellationToken cancellationToken)
    {
        var payslip = await _payslipRepo.GetByIdAsync(request.PayslipId, cancellationToken);
        _ = payslip ?? throw new NotFoundException(_t["Payslip with ID {0} not found.", request.PayslipId]);

        if (payslip.Status != PayslipStatus.Generated)
        {
            throw new ConflictException(_t["Payslip is not in a 'Generated' state. Current status: {0}", payslip.Status]);
        }

        payslip.Status = PayslipStatus.Paid;
        payslip.AddDomainEvent(EntityUpdatedEvent.WithEntity(payslip)); // Or a more specific PayslipPaidEvent

        await _payslipRepo.UpdateAsync(payslip, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return payslip.Id;
    }
}
