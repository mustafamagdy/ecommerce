using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR.Enums;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class TerminateEmployeeBenefitRequestHandler : IRequestHandler<TerminateEmployeeBenefitRequest, Guid>
{
    private readonly IRepositoryWithEvents<EmployeeBenefit> _employeeBenefitRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public TerminateEmployeeBenefitRequestHandler(
        IRepositoryWithEvents<EmployeeBenefit> employeeBenefitRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<TerminateEmployeeBenefitRequestHandler> localizer)
    {
        _employeeBenefitRepo = employeeBenefitRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(TerminateEmployeeBenefitRequest request, CancellationToken cancellationToken)
    {
        var employeeBenefit = await _employeeBenefitRepo.GetByIdAsync(request.EmployeeBenefitId, cancellationToken);
        _ = employeeBenefit ?? throw new NotFoundException(_t["Employee benefit enrollment with ID {0} not found.", request.EmployeeBenefitId]);

        // Check if already terminated
        if (employeeBenefit.Status == EmployeeBenefitStatus.Terminated && employeeBenefit.TerminationDate.HasValue && employeeBenefit.TerminationDate.Value <= request.TerminationDate)
        {
            throw new ConflictException(_t["Benefit is already terminated on or before the specified date."]);
        }

        employeeBenefit.TerminationDate = request.TerminationDate;
        employeeBenefit.Status = EmployeeBenefitStatus.Terminated; // Or PendingTermination if effective in future? For now, immediate Terminated.

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            employeeBenefit.Notes = string.IsNullOrWhiteSpace(employeeBenefit.Notes)
                ? $"Terminated: {request.Reason}"
                : $"{employeeBenefit.Notes}\nTerminated: {request.Reason}";
        }

        employeeBenefit.AddDomainEvent(EntityUpdatedEvent.WithEntity(employeeBenefit));
        await _employeeBenefitRepo.UpdateAsync(employeeBenefit, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return employeeBenefit.Id;
    }
}
