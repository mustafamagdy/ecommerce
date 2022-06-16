using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Authorization;
using Mapster;

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
      .Include(a => a.Subscriptions)
      .ThenInclude(a => a.Payments)
      .ThenInclude(a => a.PaymentMethod)
      .Include(a => a.Branches)
      .Where(a => request.Active == null || a.IsActive == request.Active)
      .Where(a => string.IsNullOrEmpty(request.Name) || a.Name.Contains(request.Name))
      .Where(a => string.IsNullOrEmpty(request.PhoneNumber) || a.PhoneNumber.Contains(request.PhoneNumber))
      .Where(a => request.SubscriptionStarted == null
                  || (a.Subscriptions.Any(x => x.StartDate >= request.SubscriptionStarted.From)
                      && (a.Subscriptions.Any(x => x.StartDate <= request.SubscriptionStarted.To))
                  ))
      .Where(a => request.SubscriptionExpired == null
                  || (a.Subscriptions.Any(x => x.ExpiryDate >= request.SubscriptionExpired.From)
                      && (a.Subscriptions.Any(x => x.ExpiryDate <= request.SubscriptionExpired.To))
                  ))
      .Where(a => request.Balance == null
                  || (a.Subscriptions.Sum(x => x.Balance) >= request.Balance.From
                      && a.Subscriptions.Sum(x => x.Balance) <= request.Balance.To
                  ))
      .OrderBy(a => a.Id)
      .AsSplitQuery();
}

public class SearchAllTenantsRequest : PaginationFilter, IRequest<PaginationResponse<TenantDto>>
{
  public string? Name { get; set; }
  public string? PhoneNumber { get; set; }
  public Range<decimal>? Balance { get; set; }
  public Range<DateTime>? SubscriptionStarted { get; set; }
  public Range<DateTime>? SubscriptionExpired { get; set; }
  public bool? Active { get; set; }
}

public class SearchAllTenantsRequestHandler : IRequestHandler<SearchAllTenantsRequest, PaginationResponse<TenantDto>>
{
  private readonly IReadTenantRepository<FSHTenantInfo> _repository;
  private readonly IDapperDbRepository _repo;


  public SearchAllTenantsRequestHandler(IReadTenantRepository<FSHTenantInfo> repository, IDapperDbRepository repo)
  {
    _repository = repository;
    _repo = repo;
  }

  public async Task<PaginationResponse<TenantDto>> Handle(SearchAllTenantsRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _repo.QueryAsync<TenantDto>(@"
select t.id, t.identifier, t.name, t.adminEmail, t.isActive, ts.Id, ts.ExpiryDate, ts.IsDemo,
       sp.Amount, pm.Name as 'PaymentMethodName', b.Id, b.Name, b.Description
from tenants t
         LEFT OUTER JOIN branches b on t.id = b.tenantId
         LEFT OUTER JOIN tenantsubscriptions ts on t.id = ts.tenantId
         LEFT OUTER JOIN subscriptions s on s.id = ts.subscriptionId
         LEFT OUTER JOIN subscriptionpayment sp on sp.TenantSubscriptionId = ts.Id
         LEFT OUTER JOIN rootpaymentmethods pm on pm.Id = sp.PaymentMethodId
where (@s1 <> '' or ts.startDate >= @s1)
   OR (@s1 <> '' or ts.startDate <= @s1)
group by t.id,ts.id, ts.price
having (@i = 0 or (ts.Price - sum(sp.amount)) > @i)
",
      new
      {
        i = 0,
        s1 = "2022-01-22"
      },
      cancellationToken: cancellationToken);
    // var spec = new TenantBySearchRequestSpec(request);
    // return _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);

    return null;
  }
}