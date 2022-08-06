using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class CreateBranchRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public class CreateBranchRequestHandler : IRequestHandler<CreateBranchRequest, Guid>
{
  private readonly IRepository<Branch> _repository;
  private readonly ITenantInfo _currentTenant;
  private readonly IApplicationUnitOfWork _uow;

  public CreateBranchRequestHandler(IRepository<Branch> repository, ITenantInfo currentTenant, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _currentTenant = currentTenant;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = new Branch(_currentTenant.Id, request.Name, request.Description);
    await _repository.AddAsync(branch, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return branch.Id;
  }
}

public class UpdateBranchRequest : IRequest
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public class UpdateBranchRequestHandler : IRequestHandler<UpdateBranchRequest>
{
  private readonly IRepository<Branch> _repository;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateBranchRequestHandler(IRepository<Branch> repository, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
  }

  public async Task<Unit> Handle(UpdateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repository.GetByIdAsync(request.Id, cancellationToken);
    await _repository.UpdateAsync(branch, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return Unit.Value;
  }
}