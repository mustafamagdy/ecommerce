using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class ActivateTenantRequest : IRequest<string>
{
  public string TenantId { get; set; } = default!;

  public ActivateTenantRequest(string tenantId) => TenantId = tenantId;
}

public class ActivateTenantRequestValidator : CustomValidator<ActivateTenantRequest>
{
  public ActivateTenantRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class ActivateTenantRequestHandler : IRequestHandler<ActivateTenantRequest, string>
{
  private readonly INonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IRepository<Branch> _branchRepo;
  private readonly ITenantUnitOfWork _uow;
  private readonly IApplicationUnitOfWork _appUow;
  private readonly IStringLocalizer _t;

  public ActivateTenantRequestHandler(INonAggregateRepository<FSHTenantInfo> repo,
    IStringLocalizer<ActivateTenantRequestHandler> localizer, ITenantUnitOfWork uow, IApplicationUnitOfWork appUow,
    IRepository<Branch> branchRepo)
  {
    _repo = repo;
    _t = localizer;
    _uow = uow;
    _appUow = appUow;
    _branchRepo = branchRepo;
  }

  public async Task<string> Handle(ActivateTenantRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.GetByIdAsync(request.TenantId, cancellationToken)
      ?? throw new NotFoundException($"Tenant {request.TenantId} not found");

    if (tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Activated."]);
    }

    tenant.Activate();

    await _uow.CommitAsync(cancellationToken);

    var branches = await _branchRepo.ListAsync(new TenantBranchSpec(request.TenantId), cancellationToken);
    branches.ForEach(a => a.Activate());
    await _appUow.CommitAsync(cancellationToken);

    return _t["Tenant {0} is now Activated.", request.TenantId];
  }
}