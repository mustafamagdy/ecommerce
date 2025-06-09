using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Payslip, PayslipStatus
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class CancelPayslipRequestHandler : IRequestHandler<CancelPayslipRequest, Guid>
{
    private readonly IRepositoryWithEvents<Payslip> _payslipRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public CancelPayslipRequestHandler(
        IRepositoryWithEvents<Payslip> payslipRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<CancelPayslipRequestHandler> localizer)
    {
        _payslipRepo = payslipRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(CancelPayslipRequest request, CancellationToken cancellationToken)
    {
        var payslip = await _payslipRepo.GetByIdAsync(request.PayslipId, cancellationToken);
        _ = payslip ?? throw new NotFoundException(_t["Payslip with ID {0} not found.", request.PayslipId]);

        if (payslip.Status == PayslipStatus.Generated || payslip.Status == PayslipStatus.Paid)
        {
            payslip.Status = PayslipStatus.Cancelled;
            payslip.CancellationReason = request.Reason ?? payslip.CancellationReason; // Update reason if provided
            payslip.AddDomainEvent(EntityUpdatedEvent.WithEntity(payslip)); // Or a more specific PayslipCancelledEvent

            await _payslipRepo.UpdateAsync(payslip, cancellationToken);
            await _uow.CommitAsync(cancellationToken);
        }
        else
        {
            throw new ConflictException(_t["Payslip cannot be cancelled as it is already {0}.", payslip.Status]);
        }

        return payslip.Id;
    }
}
