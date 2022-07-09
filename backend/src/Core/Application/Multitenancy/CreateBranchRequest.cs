using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;

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

  public CreateBranchRequestHandler(IRepository<Branch> repository, ITenantInfo currentTenant)
  {
    _repository = repository;
    _currentTenant = currentTenant;
  }

  public async Task<Guid> Handle(CreateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = new Branch(_currentTenant.Id, request.Name, request.Description);
    await _repository.AddAsync(branch, cancellationToken);

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

  public UpdateBranchRequestHandler(IRepository<Branch> repository)
  {
    _repository = repository;
  }

  public async Task<Unit> Handle(UpdateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repository.GetByIdAsync(request.Id, cancellationToken);
    await _repository.UpdateAsync(branch, cancellationToken);

    return Unit.Value;
  }
}