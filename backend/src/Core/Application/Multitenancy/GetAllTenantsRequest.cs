using Dapper;
using Dapper.FluentColumnMapping;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
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
  private readonly IDapperTenantConnectionAccessor _repo;


  public SearchAllTenantsRequestHandler(IReadTenantRepository<FSHTenantInfo> repository, IDapperTenantConnectionAccessor repo)
  {
    _repository = repository;
    _repo = repo;
  }

  public async Task<PaginationResponse<TenantDto>> Handle(SearchAllTenantsRequest request,
    CancellationToken cancellationToken)
  {
    string sql = @"
create temporary table if not exists tmp_tenants as
    (select t.id  as TenantId
          , t.identifier
          , t.name as TenantName
          , t.adminEmail
          , t.isActive
          , ts.id as SubscriptionId
          , ts.ExpiryDate
          , ts.IsDemo
          , sp.id as PaymentId
          , sp.Amount
          , pm.id as PaymentMethodId
          , pm.Name as PaymentMethodName
     from tenants t
              left join tenantSubscriptions ts on t.id = ts.tenantId
              left join subscriptions s on s.id = ts.subscriptionId
              left join subscriptionPayment sp on sp.TenantSubscriptionId = ts.Id
              left join rootPaymentMethods pm on pm.Id = sp.PaymentMethodId
     where
         ((@subStartedFrom is null or ts.startDate >= @subStartedFrom)
         OR (@subStartedTo is null or ts.startDate <= @subStartedTo))
       AND ((@subExpiredFrom is null or ts.expiryDate >= @subExpiredFrom)
         OR (@subExpiredTo is null or ts.expiryDate <= @subExpiredTo))
       AND (@name is null OR t.name like CONCAT('%', @name, '%'))
       AND (@phoneNumber is null OR t.phoneNumber like CONCAT('%', @phoneNumber, '%'))
     AND
         t.Id in (
         select t1.Id
         from tenants t1
             left join tenantSubscriptions ts1 on t1.id = ts1.tenantId
             left join subscriptionPayment sp1 on sp1.TenantSubscriptionId = ts1.Id
         group by t1.id, ts1.Price
         having  (@balanceFrom is null or (ts1.Price - ifnull(sum(sp1.amount),0)) >= @balanceFrom)
            AND (@balanceTo is null or (ts1.Price - ifnull(sum(sp1.amount),0)) <= @balanceFrom)
         )
     order by t.Id);

select * from tmp_tenants limit @pageSize offset @offset;
select count(*) from tmp_tenants;
                       ";

    var param = new
    {
      name = request.Name,
      phoneNumber = request.PhoneNumber,
      balanceFrom = request.Balance?.From,
      balanceTo = request.Balance?.To,
      subStartedFrom = request.SubscriptionStarted?.From,
      subStartedTo = request.SubscriptionStarted?.To,
      subExpiredFrom = request.SubscriptionExpired?.From,
      subExpiredTo = request.SubscriptionExpired?.To,
      pageSize = request.PageSize,
      offset = request.PageNumber * request.PageSize
    };

    using var db = await _repo.GetDbConnection(cancellationToken);


    var mappings = new ColumnMappingCollection();
    mappings.RegisterType<TenantDto>()
      .MapProperty(x => x.Id).ToColumn("TenantId")
      .MapProperty(x => x.AdminEmail).ToColumn("adminEmail")
      .MapProperty(x => x.IsActive).ToColumn("isActive")
      .MapProperty(x => x.Name).ToColumn("TenantName");

    mappings.RegisterType<TenantSubscriptionDto>()
      .MapProperty(x => x.Id).ToColumn("SubscriptionId")
      .MapProperty(x => x.TenantId).ToColumn("TenantId")
      .MapProperty(x => x.ExpiryDate).ToColumn("ExpiryDate")
      .MapProperty(x => x.IsDemo).ToColumn("IsDemo");

    mappings.RegisterType<SubscriptionPaymentDto>()
      .MapProperty(x => x.Amount).ToColumn("Amount")
      .MapProperty(x => x.PaymentMethodId).ToColumn("PaymentMethodId")
      .MapProperty(x => x.PaymentMethodId).ToColumn("PaymentMethodId")
      .MapProperty(x => x.PaymentMethodName).ToColumn("PaymentMethodName");

    mappings.RegisterWithDapper();

    using var multiResult = await db.QueryMultipleAsync(sql, param);

    var result = new Dictionary<string, TenantDto>();
    var subs = new Dictionary<Guid, TenantSubscriptionDto>();
    multiResult.Read<TenantDto, TenantSubscriptionDto, SubscriptionPaymentDto, TenantDto>(
      (t, sub, pmt) =>
      {
        if (!result.ContainsKey(t.Id))
        {
          result.Add(t.Id, t);
        }

        var tenant = result[t.Id];
        if (sub == null || sub.Id == Guid.Empty)
        {
          return t;
        }

        if (!subs.ContainsKey(sub.Id))
        {
          subs.Add(sub.Id, sub);
        }

        var subscription = subs[sub.Id];
        subscription.TenantId = tenant.Id;

        if (pmt != null && pmt.PaymentMethodId != Guid.Empty)
        {
          subscription.Payments.Add(pmt);
        }

        int subIdx = tenant.Subscriptions.FindIndex(a => a.Id == subscription.Id);
        if (subIdx == -1)
          tenant.Subscriptions.Add(subscription);
        else
          tenant.Subscriptions[subIdx] = subscription;

        return t;
      },
      splitOn: "TenantId, SubscriptionId, PaymentId"
    );

    int totalCount = multiResult.ReadSingle<int>();

    return new PaginationResponse<TenantDto>(result.Values.ToList(), totalCount, request.PageNumber, request.PageSize);
  }
}