using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;

namespace FSH.WebApi.Application.Multitenancy;

public class SearchBranchRequest : IRequest<BranchDto>
{
  public Guid? TenantId { get; set; }
  public string? Name { get; set; }
}

public class BranchByQuerySpec : Specification<Branch>, ISingleResultSpecification
{
  public BranchByQuerySpec(SearchBranchRequest request) => Query
    .Where(a => request.TenantId == null || a.TenantId == request.TenantId.ToString())
    .Where(a => string.IsNullOrWhiteSpace(request.Name) || a.Name.Contains(request.Name));
}

public class SearchBranchRequestHandler : IRequestHandler<SearchBranchRequest, BranchDto>
{
  private readonly IReadRepository<Branch> _repository;

  public SearchBranchRequestHandler(IReadRepository<Branch> repository)
  {
    _repository = repository;
  }

  public async Task<BranchDto> Handle(SearchBranchRequest request, CancellationToken cancellationToken)
  {
    var spec = new BranchByQuerySpec(request);
    return await _repository.GetBySpecAsync((ISpecification<Branch, BranchDto>)spec, cancellationToken);
  }
}