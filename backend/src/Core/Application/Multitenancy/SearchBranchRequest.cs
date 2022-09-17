using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class SearchBranchRequest : IRequest<List<BranchDto>>
{
  public string? Name { get; set; }
}

public class BranchByQuerySpec : Specification<Branch, BranchDto>
{
  public BranchByQuerySpec(SearchBranchRequest request) =>
    Query.Where(a => string.IsNullOrEmpty(request.Name) || a.Name.Contains(request.Name));
}

public class SearchBranchRequestHandler : IRequestHandler<SearchBranchRequest, List<BranchDto>>
{
  private readonly IReadRepository<Branch> _repository;

  public SearchBranchRequestHandler(IReadRepository<Branch> repository)
  {
    _repository = repository;
  }

  public async Task<List<BranchDto>> Handle(SearchBranchRequest request, CancellationToken cancellationToken)
  {
    var spec = new BranchByQuerySpec(request);
    return await _repository.ListAsync(spec, cancellationToken);
  }
}