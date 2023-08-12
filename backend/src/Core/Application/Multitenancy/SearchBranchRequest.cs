using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class SearchBranchRequest : PaginationFilter,IRequest<PaginationResponse<BranchDto>>
{
  public string? Name { get; set; }
}

public class BranchByQuerySpec : EntitiesByPaginationFilterSpec<Branch, BranchDto>
{
  public BranchByQuerySpec(SearchBranchRequest request): base(request) =>
    Query.Where(a => string.IsNullOrEmpty(request.Name) || a.Name.Contains(request.Name));
}

public class SearchBranchRequestHandler : IRequestHandler<SearchBranchRequest, PaginationResponse<BranchDto>>
{
  private readonly IReadRepository<Branch> _repository;

  public SearchBranchRequestHandler(IReadRepository<Branch> repository)
  {
    _repository = repository;
  }

  public async Task<PaginationResponse<BranchDto>> Handle(SearchBranchRequest request, CancellationToken cancellationToken)
  {
    var spec = new BranchByQuerySpec(request);
    return await _repository.PaginatedListAsync(spec,request.PageNumber,request.PageSize, cancellationToken);
  }
}