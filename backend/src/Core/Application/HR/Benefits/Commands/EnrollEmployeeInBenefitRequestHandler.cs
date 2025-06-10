using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Employee
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus
using FSH.WebApi.Application.HR.Benefits.Specifications; // For ActiveEmployeeBenefitByPlanSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class EnrollEmployeeInBenefitRequestHandler : IRequestHandler<EnrollEmployeeInBenefitRequest, Guid>
{
    private readonly IRepositoryWithEvents<EmployeeBenefit> _employeeBenefitRepo;
    private readonly IReadRepository<Employee> _employeeRepo;
    private readonly IReadRepository<BenefitPlan> _benefitPlanRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public EnrollEmployeeInBenefitRequestHandler(
        IRepositoryWithEvents<EmployeeBenefit> employeeBenefitRepo,
        IReadRepository<Employee> employeeRepo,
        IReadRepository<BenefitPlan> benefitPlanRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<EnrollEmployeeInBenefitRequestHandler> localizer)
    {
        _employeeBenefitRepo = employeeBenefitRepo;
        _employeeRepo = employeeRepo;
        _benefitPlanRepo = benefitPlanRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(EnrollEmployeeInBenefitRequest request, CancellationToken cancellationToken)
    {
        // Validate EmployeeId
        var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId, cancellationToken);
        _ = employee ?? throw new NotFoundException(_t["Employee with ID {0} not found.", request.EmployeeId]);

        // Validate BenefitPlanId (exists and is active)
        var benefitPlan = await _benefitPlanRepo.GetByIdAsync(request.BenefitPlanId, cancellationToken);
        _ = benefitPlan ?? throw new NotFoundException(_t["Benefit plan with ID {0} not found.", request.BenefitPlanId]);
        if (!benefitPlan.IsActive)
        {
            throw new ConflictException(_t["Benefit plan '{0}' is not currently active.", benefitPlan.PlanName]);
        }

        // Check for existing active enrollment for this employee in this plan
        var activeEnrollmentSpec = new ActiveEmployeeBenefitByPlanSpec(request.EmployeeId, request.BenefitPlanId);
        var existingActiveEnrollment = await _employeeBenefitRepo.FirstOrDefaultAsync(activeEnrollmentSpec, cancellationToken);
        if (existingActiveEnrollment is not null)
        {
            throw new ConflictException(_t["Employee {0} is already actively enrolled in benefit plan '{1}'.", employee.FirstName + " " + employee.LastName, benefitPlan.PlanName]);
        }

        var employeeBenefit = new EmployeeBenefit(request.EmployeeId, request.BenefitPlanId, request.EnrollmentDate, request.EffectiveDate)
        {
            EmployeeContributionOverride = request.EmployeeContributionOverride,
            EmployerContributionOverride = request.EmployerContributionOverride,
            Notes = request.Notes,
            Status = EmployeeBenefitStatus.Active // Or PendingEnrollment if an approval workflow is needed
        };

        employeeBenefit.AddDomainEvent(EntityCreatedEvent.WithEntity(employeeBenefit));
        await _employeeBenefitRepo.AddAsync(employeeBenefit, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return employeeBenefit.Id;
    }
}
