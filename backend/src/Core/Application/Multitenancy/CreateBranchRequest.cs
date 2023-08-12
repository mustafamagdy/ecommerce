using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Common.Events;
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

    branch.AddDomainEvent(EntityCreatedEvent.WithEntity(branch));

    await _repository.AddAsync(branch, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return branch.Id;
  }
}