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

public class TenantBySearchRequestSpec : EntitiesByPaginationFilterSpec<FSHTenantInfo, TenantDto>
{
  public TenantBySearchRequestSpec(SearchAllTenantsRequest request)
    : base(request) =>
    Query
      .Include(a => a.Subscriptions)
      .ThenInclude(a => a.Subscription)
      .Include(a => a.Branches)
      .Where(a => request.Active == null || a.IsActive == request.Active)
      // .Where(a => string.IsNullOrEmpty(request.Name) || a.Name.Contains(request.Name))
      // .Where(a => string.IsNullOrEmpty(request.PhoneNumber) || a.PhoneNumber.Contains(request.PhoneNumber))
      // .Where(a => request.SubscriptionStarted == null
      //             || (a.Subscriptions.Any(x => x.StartDate >= request.SubscriptionStarted.From)
      //                 && (a.Subscriptions.Any(x => x.StartDate <= request.SubscriptionStarted.To))
      //             ))
      // .Where(a => request.SubscriptionExpired == null
      //             || (a.Subscriptions.Any(x => x.ExpiryDate >= request.SubscriptionExpired.From)
      //                 && (a.Subscriptions.Any(x => x.ExpiryDate <= request.SubscriptionExpired.To))
      //             ))
      // .Where(a => request.Balance == null
      //             || (a.Subscriptions.Sum(x => x.Balance) >= request.Balance.From
      //                 && a.Subscriptions.Sum(x => x.Balance) <= request.Balance.To
      //             ))
      .OrderBy(a => a.Id);
}

public class SearchAllTenantsRequest : PaginationFilter, IRequest<PaginationResponse<TenantDto>>
{
  public string? Name { get; private set; }
  public string? PhoneNumber { get; private set; }
  public Range<decimal>? Balance { get; private set; }
  public Range<DateTime>? SubscriptionStarted { get; private set; }
  public Range<DateTime>? SubscriptionExpired { get; private set; }
  public bool? Active { get; private set; }
}

public class SearchAllTenantsRequestHandler : IRequestHandler<SearchAllTenantsRequest, PaginationResponse<TenantDto>>
{
  private readonly IReadTenantRepository<FSHTenantInfo> _repository;


  public SearchAllTenantsRequestHandler(IReadTenantRepository<FSHTenantInfo> repository) =>
    _repository = repository;

  public Task<PaginationResponse<TenantDto>> Handle(SearchAllTenantsRequest request,
    CancellationToken cancellationToken)
  {
    var spec = new TenantBySearchRequestSpec(request);
    return _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}