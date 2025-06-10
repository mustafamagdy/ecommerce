using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR.Benefits;
using FSH.WebApi.Application.HR.Benefits.Specifications; // For BenefitPlanByCodeSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class DefineBenefitPlanRequestHandler : IRequestHandler<DefineBenefitPlanRequest, Guid>
{
    private readonly IRepositoryWithEvents<BenefitPlan> _benefitPlanRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public DefineBenefitPlanRequestHandler(
        IRepositoryWithEvents<BenefitPlan> benefitPlanRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<DefineBenefitPlanRequestHandler> localizer)
    {
        _benefitPlanRepo = benefitPlanRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(DefineBenefitPlanRequest request, CancellationToken cancellationToken)
    {
        // Check for PlanCode uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.PlanCode))
        {
            var spec = new BenefitPlanByCodeSpec(request.PlanCode);
            var existingPlanWithCode = await _benefitPlanRepo.FirstOrDefaultAsync(spec, cancellationToken);
            if (existingPlanWithCode is not null && existingPlanWithCode.Id != request.Id) // If ID matches, it's an update to itself
            {
                throw new ConflictException(_t["A benefit plan with code '{0}' already exists.", request.PlanCode]);
            }
        }

        BenefitPlan? benefitPlan;
        bool isUpdate = request.Id.HasValue;

        if (isUpdate)
        {
            benefitPlan = await _benefitPlanRepo.GetByIdAsync(request.Id!.Value, cancellationToken);
            _ = benefitPlan ?? throw new NotFoundException(_t["Benefit plan with ID {0} not found.", request.Id.Value]);

            // Update properties
            benefitPlan.PlanName = request.PlanName;
            benefitPlan.PlanCode = request.PlanCode; // Can be updated
            benefitPlan.Description = request.Description;
            benefitPlan.Provider = request.Provider;
            benefitPlan.Type = request.Type;
            benefitPlan.ContributionAmountEmployee = request.ContributionAmountEmployee;
            benefitPlan.ContributionAmountEmployer = request.ContributionAmountEmployer;
            benefitPlan.IsActive = request.IsActive;

            benefitPlan.AddDomainEvent(EntityUpdatedEvent.WithEntity(benefitPlan));
            await _benefitPlanRepo.UpdateAsync(benefitPlan, cancellationToken);
        }
        else
        {
            benefitPlan = new BenefitPlan(request.PlanName, request.Type)
            {
                PlanCode = request.PlanCode,
                Description = request.Description,
                Provider = request.Provider,
                ContributionAmountEmployee = request.ContributionAmountEmployee,
                ContributionAmountEmployer = request.ContributionAmountEmployer,
                IsActive = request.IsActive
            };
            benefitPlan.AddDomainEvent(EntityCreatedEvent.WithEntity(benefitPlan));
            await _benefitPlanRepo.AddAsync(benefitPlan, cancellationToken);
        }

        await _uow.CommitAsync(cancellationToken);
        return benefitPlan.Id;
    }
}
