using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class ActivateBranchRequest : IRequest<Unit>
{
  public Guid BranchId { get; set; }

  public ActivateBranchRequest(Guid branchId) => BranchId = branchId;
}

public class ActivateBranchRequestValidator : CustomValidator<ActivateBranchRequest>
{
  public ActivateBranchRequestValidator() =>
    RuleFor(t => t.BranchId)
      .NotEmpty();
}

public class ActivateBranchRequestHandler : IRequestHandler<ActivateBranchRequest, Unit>
{
  private readonly IRepository<Branch> _repo;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public ActivateBranchRequestHandler(IRepository<Branch> repo, IStringLocalizer<ActivateBranchRequestHandler> localizer, IApplicationUnitOfWork uow)
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
  public Guid BranchId { get; set; }

  public DeactivateBranchRequest(Guid branchId) => BranchId = branchId;
}

public class DeactivateBranchRequestValidator : CustomValidator<DeactivateBranchRequest>
{
  public DeactivateBranchRequestValidator() =>
    RuleFor(t => t.BranchId)
      .NotEmpty();
}

public class DeactivateBranchRequestHandler : IRequestHandler<DeactivateBranchRequest, Unit>
{
  private readonly IRepository<Branch> _repo;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public DeactivateBranchRequestHandler(IRepository<Branch> repo, IStringLocalizer<DeactivateBranchRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _repo = repo;
    _t = localizer;
    _uow = uow;
  }

  public async Task<Unit> Handle(DeactivateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repo.GetByIdAsync(request.BranchId, cancellationToken)
                 ?? throw new NotFoundException($"Branch {request.BranchId} not found");

    if (!branch.Active)
    {
      throw new ConflictException(_t["Tenant is already deactivated."]);
    }

    branch.Deactivate();
    await _uow.CommitAsync(cancellationToken);

    return Unit.Value;
  }
}