using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class ActivateBranchRequest : IRequest<Unit>
{
  public string BranchId { get; set; }

  public ActivateBranchRequest(string tenantId) => BranchId = tenantId;
}

public class ActivateBranchRequestValidator : CustomValidator<ActivateBranchRequest>
{
  public ActivateBranchRequestValidator() =>
    RuleFor(t => t.BranchId)
      .NotEmpty();
}

public class ActivateBranchRequestHandler : IRequestHandler<ActivateBranchRequest, Unit>
{
  private readonly INonAggregateRepository<Branch> _repo;
  private readonly ITenantUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public ActivateBranchRequestHandler(INonAggregateRepository<Branch> repo, IStringLocalizer<ActivateBranchRequestHandler> localizer, ITenantUnitOfWork uow)
  {
    _repo = repo;
    _t = localizer;
    _uow = uow;
  }

  public async Task<Unit> Handle(ActivateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repo.GetByIdAsync(request.BranchId, cancellationToken)
                 ?? throw new NotFoundException($"Branch {request.BranchId} not found");

    if (branch.Active)
    {
      throw new ConflictException(_t["Tenant is already Activated."]);
    }

    branch.Activate();
    await _uow.CommitAsync(cancellationToken);
    return Unit.Value;
  }
}

public class DeactivateBranchRequest : IRequest<Unit>
{
  public string BranchId { get; set; }

  public DeactivateBranchRequest(string tenantId) => BranchId = tenantId;
}

public class DeactivateBranchRequestValidator : CustomValidator<DeactivateBranchRequest>
{
  public DeactivateBranchRequestValidator() =>
    RuleFor(t => t.BranchId)
      .NotEmpty();
}

public class DeactivateBranchRequestHandler : IRequestHandler<DeactivateBranchRequest, Unit>
{
  private readonly INonAggregateRepository<Branch> _repo;
  private readonly ITenantUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public DeactivateBranchRequestHandler(INonAggregateRepository<Branch> repo, IStringLocalizer<DeactivateBranchRequestHandler> localizer, ITenantUnitOfWork uow)
  {
    _repo = repo;
    _t = localizer;
    _uow = uow;
  }

  public async Task<Unit> Handle(DeactivateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repo.GetByIdAsync(request.BranchId, cancellationToken)
                 ?? throw new NotFoundException($"Branch {request.BranchId} not found");

    if (branch.Active)
    {
      throw new ConflictException(_t["Tenant is already deactivated."]);
    }

    branch.Deactivate();
    await _uow.CommitAsync(cancellationToken);
    return Unit.Value;
  }
}