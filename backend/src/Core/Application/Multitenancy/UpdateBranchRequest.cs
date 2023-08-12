using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class UpdateBranchRequest : IRequest<BranchDto>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public class UpdateBranchRequestHandler : IRequestHandler<UpdateBranchRequest,BranchDto>
{
  private readonly IRepository<Branch> _repository;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateBranchRequestHandler(IRepository<Branch> repository, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
  }

  public async Task<BranchDto> Handle(UpdateBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repository.GetByIdAsync(request.Id, cancellationToken);
    branch.Update(request.Name, request.Description);
    await _repository.UpdateAsync(branch, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return branch.Adapt<BranchDto>();
  }
}
