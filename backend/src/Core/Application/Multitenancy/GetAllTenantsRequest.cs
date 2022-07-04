using Dapper;
using Dapper.FluentColumnMapping;
using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

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
  private readonly IDapperTenantConnectionAccessor _repo;

  public SearchAllTenantsRequestHandler(IDapperTenantConnectionAccessor repo)
  {
    _repo = repo;
  }

  public async Task<PaginationResponse<TenantDto>> Handle(SearchAllTenantsRequest request,
    CancellationToken cancellationToken)
  {
    string sql = @"
-- set @subStartedFrom = null;
-- set @subStartedTo = null;
-- set @subExpiredFrom = null;
-- set @subExpiredTo = null;
-- set @name = null;
-- set @phoneNumber = null;
-- set @balanceFrom = null;
-- set @balanceTo = null;

create temporary table if not exists tmp_tenants as
    (select t.Id  as TenantId
          , t.Identifier
          , t.name as TenantName
          , t.adminEmail
          , t.active
          , std.Id as ProdSubscription_Id
          , std_sh.Price as ProdSubscription_Price
          , std_sh.StartDate as ProdSubscription_StartDate
          , std_sh.ExpiryDate as ProdSubscription_ExpiryDate
          , demo.Id as DemoSubscription_Id
          , demo_sh.StartDate as DemoSubscription_StartDate
          , demo_sh.ExpiryDate as DemoSubscription_ExpiryDate
          , train.Id as TrainSubscription_Id
          , train_sh.StartDate as TrainSubscription_StartDate
          , train_sh.ExpiryDate as TrainSubscription_ExpiryDate
          , sp.Id as PaymentId
          , sp.Amount as Amount
          , pm.Id as PaymentMethodId
          , pm.Name as PaymentMethodName
          , b.Id as BranchId
          , b.Name as BranchName
          , b.Description as BranchDescription
     from Tenants t
              left join Branches b on b.TenantId = t.Id
              left join Subscription std on std.Id = t.ProdSubscriptionId
              left join Subscription demo on demo.Id = t.DemoSubscriptionId
              left join Subscription train on train.Id = t.TrainSubscriptionId
              left join SubscriptionHistories std_sh on t.Id = std_sh.tenantId and std.Id = std_sh.StandardSubscriptionId
              left join SubscriptionHistories demo_sh on t.Id = demo_sh.tenantId and demo.Id = demo_sh.StandardSubscriptionId
              left join SubscriptionHistories train_sh on t.Id = train_sh.tenantId and train.Id = train_sh.StandardSubscriptionId
              left join SubscriptionPayments sp on sp.SubscriptionId = std.Id
              left join RootPaymentMethods pm on pm.Id = sp.PaymentMethodId
     where
             t.Name <> 'root'
       AND ((@subStartedFrom is null or std_sh.startDate >= @subStartedFrom) OR (@subStartedTo is null or std_sh.startDate <= @subStartedTo))
       AND ((@subExpiredFrom is null or std_sh.expiryDate >= @subExpiredFrom) OR (@subExpiredTo is null or std_sh.expiryDate <= @subExpiredTo))
       AND (@name is null OR t.name like CONCAT('%', @name, '%'))
       AND (@phoneNumber is null OR t.phoneNumber like CONCAT('%', @phoneNumber, '%'))
       AND
             t.Id in (
             select t1.Id
             from tenants t1
                      left join Subscription stdSub on t1.ProdSubscriptionId = stdSub.id
                      left join SubscriptionPayments sp1 on sp1.SubscriptionId = stdSub.Id
             group by t1.Id, stdSub.Price
             having  (@balanceFrom is null or (stdSub.Price - ifnull(sum(sp1.amount),0)) >= @balanceFrom)
                AND (@balanceTo is null or (stdSub.Price - ifnull(sum(sp1.amount),0)) <= @balanceTo)
         )
     order by t.Id);

-- select * from tmp_tenants;
select * from tmp_tenants; limit @pageSize offset @offset;
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
      .MapProperty(x => x.Active).ToColumn("active")
      .MapProperty(x => x.Name).ToColumn("TenantName");

    mappings.RegisterType<ProdTenantSubscriptionDto>()
      .MapProperty(x => x.Id).ToColumn("SubscriptionId")
      .MapProperty(x => x.TenantId).ToColumn("TenantId")
      .MapProperty(x => x.ExpiryDate).ToColumn("ExpiryDate");

    mappings.RegisterType<DemoTenantSubscriptionDto>()
      .MapProperty(x => x.Id).ToColumn("SubscriptionId")
      .MapProperty(x => x.TenantId).ToColumn("TenantId")
      .MapProperty(x => x.ExpiryDate).ToColumn("ExpiryDate");

    mappings.RegisterType<TrainTenantSubscriptionDto>()
      .MapProperty(x => x.Id).ToColumn("SubscriptionId")
      .MapProperty(x => x.TenantId).ToColumn("TenantId")
      .MapProperty(x => x.ExpiryDate).ToColumn("ExpiryDate");

    mappings.RegisterType<SubscriptionPaymentDto>()
      .MapProperty(x => x.Id).ToColumn("PaymentId")
      .MapProperty(x => x.Amount).ToColumn("Amount")
      .MapProperty(x => x.PaymentMethodId).ToColumn("PaymentMethodId")
      .MapProperty(x => x.PaymentMethodName).ToColumn("PaymentMethodName");

    mappings.RegisterType<BranchDto>()
      .MapProperty(x => x.Id).ToColumn("BranchId")
      .MapProperty(x => x.Name).ToColumn("branchName")
      .MapProperty(x => x.Description).ToColumn("branchDescription");

    mappings.RegisterWithDapper();

    using var multiResult = await db.QueryMultipleAsync(sql, param);

    var result = new Dictionary<string, TenantDto>();
    var prod_subs = new Dictionary<Guid, ProdTenantSubscriptionDto>();
    var demo_subs = new Dictionary<Guid, DemoTenantSubscriptionDto>();
    var train_subs = new Dictionary<Guid, TrainTenantSubscriptionDto>();
    var branches = new Dictionary<Guid, BranchDto>();

    multiResult.Read<TenantDto, ProdTenantSubscriptionDto, DemoTenantSubscriptionDto, TrainTenantSubscriptionDto, SubscriptionPaymentDto, BranchDto, TenantDto>(
      (t, prod_sub, demo_sub, train_sub, pmt, b) =>
      {
        if (!result.ContainsKey(t.Id))
        {
          result.Add(t.Id, t);
        }

        var tenant = result[t.Id];

        if (b != null && b.Id != Guid.Empty)
        {
          if (!branches.ContainsKey(b.Id))
          {
            branches.Add(b.Id, b);
            tenant.Branches.Add(b);
          }
        }

        if (prod_sub != null && prod_sub.Id != Guid.Empty)
        {
          if (!prod_subs.ContainsKey(prod_sub.Id))
          {
            prod_sub.TenantId = tenant.Id;
            prod_subs.Add(prod_sub.Id, prod_sub);
            tenant.ProdSubscription = prod_sub;
          }
        }

        if (demo_sub != null && demo_sub.Id != Guid.Empty)
        {
          if (!demo_subs.ContainsKey(demo_sub.Id))
          {
            demo_sub.TenantId = tenant.Id;
            demo_subs.Add(demo_sub.Id, demo_sub);
            tenant.DemoSubscription = demo_sub;
          }
        }

        if (train_sub != null && train_sub.Id != Guid.Empty)
        {
          if (!train_subs.ContainsKey(train_sub.Id))
          {
            train_sub.TenantId = tenant.Id;
            train_subs.Add(train_sub.Id, train_sub);
            tenant.TrainSubscription = train_sub;
          }
        }

        var subscription = prod_subs[prod_sub.Id];
        if (pmt != null && pmt.PaymentMethodId != Guid.Empty)
        {
          subscription.Payments.Add(pmt);
        }

        return t;
      },
      splitOn: "TenantId, SubscriptionId, PaymentId, BranchId");

    int totalCount = multiResult.ReadSingle<int>();

    return new PaginationResponse<TenantDto>(result.Values.ToList(), totalCount, request.PageNumber, request.PageSize);
  }
}