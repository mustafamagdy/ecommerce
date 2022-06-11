using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Authorization;

namespace FSH.WebApi.Application.Multitenancy;

public class Range<T>
  where T : struct
{
  public T From { get; set; }
  public T To { get; set; }

  public Range<T> Between(T from, T to) => new() { From = from, To = to };
}

public class TenantBySearchRequestSpec : Specification<FSHTenantInfo>
{
//Todo: complete
  public TenantBySearchRequestSpec(SearchAllTenantsRequest request) => Query.Include(a => a.ActiveSubscriptions)
}

public class SearchAllTenantsRequest : PaginationFilter, IRequest<PaginationResponse<TenantDto>>
{
  public string? Name { get; private set; }
  public string PhoneNumber { get; private set; }
  public Range<decimal>? Balance { get; private set; }
  public Range<DateTime>? SubscriptionStarted { get; private set; }
  public Range<DateTime>? SubscriptionExpired { get; private set; }
  public bool? Active { get; private set; }
}

public class SearchAllTenantsRequestHandler : IRequestHandler<SearchAllTenantsRequest, PaginationResponse<TenantDto>>
{
  private readonly ITenantService _tenantService;

  public SearchAllTenantsRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

  public Task<PaginationResponse<TenantDto>> Handle(SearchAllTenantsRequest request, CancellationToken cancellationToken)
  {
    var spec = new TenantBySearchRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}