using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class DeactivateTenantRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;

  public DeactivateTenantRequest(string tenantId) => TenantId = tenantId;
}

public class DeactivateTenantRequestValidator : CustomValidator<DeactivateTenantRequest>
{
  public DeactivateTenantRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class DeactivateTenantRequestHandler : IRequestHandler<DeactivateTenantRequest, string>
{
  private readonly INonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IRepository<Branch> _branchRepo;
  private readonly IStringLocalizer _t;

  private readonly ITenantUnitOfWork _uow;
  private readonly IApplicationUnitOfWork _appUow;

  public DeactivateTenantRequestHandler(INonAggregateRepository<FSHTenantInfo> repo,
    IStringLocalizer<ActivateTenantRequestHandler> localizer, IRepository<Branch> branchRepo,
    IApplicationUnitOfWork appUow, ITenantUnitOfWork uow)
  {
    _repo = repo;
    _t = localizer;
    _branchRepo = branchRepo;
    _appUow = appUow;
    _uow = uow;
  }

  public async Task<string> Handle(DeactivateTenantRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.GetByIdAsync(request.TenantId, cancellationToken)
                 ?? throw new NotFoundException($"Tenant {request.TenantId} not found");

    if (!tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Deactivated."]);
    }

    tenant.DeActivate();

    await _uow.CommitAsync(cancellationToken);

    var branches = await _branchRepo.ListAsync(new TenantBranchSpec(request.TenantId), cancellationToken);
    branches.ForEach(a => a.Deactivate());
    await _appUow.CommitAsync(cancellationToken);


    return _t[$"Tenant {0} is now Deactivated.", request.TenantId];
  }
}