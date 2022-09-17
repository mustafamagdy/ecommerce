using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Application.Multitenancy;

public class RenewSubscriptionRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;
  public SubscriptionType SubscriptionType { get; set; }
}

public class RenewSubscriptionRequestValidator : CustomValidator<RenewSubscriptionRequest>
{
  public RenewSubscriptionRequestValidator()
  {
    RuleFor(t => t.TenantId).NotEmpty();
    RuleFor(t => t.SubscriptionType).NotNull();
  }
}

public class RenewSubscriptionRequestHandler : IRequestHandler<RenewSubscriptionRequest, string>
{
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
  private readonly INonAggregateRepository<TenantSubscription> _tenantSubscriptionRepo;
  private readonly ISystemTime _systemTime;
  private readonly ITenantUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public RenewSubscriptionRequestHandler(ISystemTime systemTime, ITenantUnitOfWork uow,
    IStringLocalizer<RenewSubscriptionRequestHandler> localizer, IReadNonAggregateRepository<FSHTenantInfo> tenantRepo,
    INonAggregateRepository<TenantSubscription> tenantSubscriptionRepo)
  {
    _systemTime = systemTime;
    _uow = uow;
    _t = localizer;
    _tenantRepo = tenantRepo;
    _tenantSubscriptionRepo = tenantSubscriptionRepo;
  }

  public async Task<string> Handle(RenewSubscriptionRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _tenantRepo.GetByIdAsync(request.TenantId, cancellationToken);
    var subscriptionId = request.SubscriptionType.Name switch
    {
      nameof(SubscriptionType.Standard) => tenant.ProdSubscriptionId,
      nameof(SubscriptionType.Demo) => tenant.DemoSubscriptionId,
      nameof(SubscriptionType.Train) => tenant.TrainSubscriptionId,
      _ => throw new ArgumentOutOfRangeException(nameof(request.SubscriptionType))
    };

    var tenantSubscription = await _tenantSubscriptionRepo.FirstOrDefaultAsync(
        new SingleResultSpecification<TenantSubscription>()
          .Query
          .Include(a => a.History)
          .Include(a => a.CurrentPackage)
          .Where(a => a.Id == subscriptionId!.Value)
          .Specification, cancellationToken)
      .ConfigureAwait(false);

    tenantSubscription.Renew(_systemTime.Now);

    tenantSubscription.AddDomainEvent(new SubscriptionRenewedEvent(tenant, tenantSubscription));

    await _uow.CommitAsync(cancellationToken);

    return _t["Subscription {0} renewed. Now Valid till {1}.", request.SubscriptionType.Name, tenantSubscription.ExpiryDate];
  }
}