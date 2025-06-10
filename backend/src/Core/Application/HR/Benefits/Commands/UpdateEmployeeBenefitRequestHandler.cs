using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR.Enums;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class UpdateEmployeeBenefitRequestHandler : IRequestHandler<UpdateEmployeeBenefitRequest, Guid>
{
    private readonly IRepositoryWithEvents<EmployeeBenefit> _employeeBenefitRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateEmployeeBenefitRequestHandler(
        IRepositoryWithEvents<EmployeeBenefit> employeeBenefitRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateEmployeeBenefitRequestHandler> localizer)
    {
        _employeeBenefitRepo = employeeBenefitRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateEmployeeBenefitRequest request, CancellationToken cancellationToken)
    {
        var employeeBenefit = await _employeeBenefitRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = employeeBenefit ?? throw new NotFoundException(_t["Employee benefit enrollment with ID {0} not found.", request.Id]);

        // Update fields if provided in the request
        if (request.EnrollmentDate.HasValue)
            employeeBenefit.EnrollmentDate = request.EnrollmentDate.Value;
        if (request.EffectiveDate.HasValue)
            employeeBenefit.EffectiveDate = request.EffectiveDate.Value;

        // Handle TerminationDate: if null in request, it clears the date.
        if (request.TerminationDate.HasValue || (request.TerminationDate is null && employeeBenefit.TerminationDate is not null) )
            employeeBenefit.TerminationDate = request.TerminationDate;

        // Handle Overrides: if null in request, it clears the override (meaning use plan default)
        if (request.EmployeeContributionOverride.HasValue || (request.EmployeeContributionOverride is null && employeeBenefit.EmployeeContributionOverride is not null))
            employeeBenefit.EmployeeContributionOverride = request.EmployeeContributionOverride;
        if (request.EmployerContributionOverride.HasValue || (request.EmployerContributionOverride is null && employeeBenefit.EmployerContributionOverride is not null))
            employeeBenefit.EmployerContributionOverride = request.EmployerContributionOverride;

        if (request.Status.HasValue)
            employeeBenefit.Status = request.Status.Value;

        // Handle Notes: if null in request, it clears notes. If provided, it overwrites.
        if (request.Notes is not null || (request.Notes is null && employeeBenefit.Notes is not null))
            employeeBenefit.Notes = request.Notes;


        // Business logic for status changes based on dates
        if (employeeBenefit.TerminationDate.HasValue && employeeBenefit.TerminationDate.Value <= DateTime.UtcNow.Date)
        {
            if(employeeBenefit.Status != EmployeeBenefitStatus.Terminated) // only change if not already terminated
            {
                 employeeBenefit.Status = EmployeeBenefitStatus.Terminated;
            }
        }
        // Add more complex status transition logic here if needed (e.g., from PendingEnrollment to Active on EffectiveDate)

        employeeBenefit.AddDomainEvent(EntityUpdatedEvent.WithEntity(employeeBenefit));
        await _employeeBenefitRepo.UpdateAsync(employeeBenefit, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return employeeBenefit.Id;
    }
}
