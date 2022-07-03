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
set @subStartedFrom = null;
set @subStartedTo = null;
set @subExpiredFrom = null;
set @subExpiredTo = null;
set @name = null;
set @phoneNumber = null;
set @balanceFrom = null;
set @balanceTo = null;

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

select * from tmp_tenants; -- limit @pageSize offset @offset;
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
    //
    // var mappings = new ColumnMappingCollection();
    // mappings.RegisterType<TenantDto>()
    //   .MapProperty(x => x.Id).ToColumn("TenantId")
    //   .MapProperty(x => x.AdminEmail).ToColumn("adminEmail")
    //   .MapProperty(x => x.IsActive).ToColumn("isActive")
    //   .MapProperty(x => x.Name).ToColumn("TenantName");
    //
    // mappings.RegisterType<TenantSubscriptionDto>()
    //   .MapProperty(x => x.Id).ToColumn("SubscriptionId")
    //   .MapProperty(x => x.TenantId).ToColumn("TenantId")
    //   .MapProperty(x => x.ExpiryDate).ToColumn("ExpiryDate")
    //   .MapProperty(x => x.IsDemo).ToColumn("IsDemo");
    //
    // mappings.RegisterType<SubscriptionPaymentDto>()
    //   .MapProperty(x => x.Amount).ToColumn("Amount")
    //   .MapProperty(x => x.PaymentMethodId).ToColumn("PaymentMethodId")
    //   .MapProperty(x => x.PaymentMethodId).ToColumn("PaymentMethodId")
    //   .MapProperty(x => x.PaymentMethodName).ToColumn("PaymentMethodName");
    //
    // mappings.RegisterType<BranchDto>()
    //   .MapProperty(x => x.Id).ToColumn("BranchId")
    //   .MapProperty(x => x.Name).ToColumn("branchName")
    //   .MapProperty(x => x.Description).ToColumn("branchDescription");
    //
    // mappings.RegisterWithDapper();
    //
    // using var multiResult = await db.QueryMultipleAsync(sql, param);
    //
    // var result = new Dictionary<string, TenantDto>();
    // var subs = new Dictionary<Guid, TenantSubscriptionDto>();
    // var branches = new Dictionary<Guid, BranchDto>();
    //
    // multiResult.Read<TenantDto, TenantSubscriptionDto, SubscriptionPaymentDto, BranchDto, TenantDto>(
    //   (t, sub, pmt, b) =>
    //   {
    //     if (!result.ContainsKey(t.Id))
    //     {
    //       result.Add(t.Id, t);
    //     }
    //
    //     var tenant = result[t.Id];
    //
    //     if (b != null && b.Id != Guid.Empty)
    //     {
    //       if (!branches.ContainsKey(b.Id))
    //       {
    //         branches.Add(b.Id, b);
    //         tenant.Branches.Add(b);
    //       }
    //     }
    //
    //     if (sub != null && sub.Id != Guid.Empty)
    //     {
    //       if (!subs.ContainsKey(sub.Id))
    //       {
    //         subs.Add(sub.Id, sub);
    //         tenant.Subscriptions.Add(sub);
    //       }
    //     }
    //
    //     var subscription = subs[sub.Id];
    //     subscription.TenantId = tenant.Id;
    //
    //     if (pmt != null && pmt.PaymentMethodId != Guid.Empty)
    //     {
    //       subscription.Payments.Add(pmt);
    //     }
    //
    //     return t;
    //   },
    //   splitOn: "TenantId, SubscriptionId, PaymentId, BranchId");

    // int totalCount = multiResult.ReadSingle<int>();
    //
    // return new PaginationResponse<TenantDto>(result.Values.ToList(), totalCount, request.PageNumber, request.PageSize);
    return null;
  }
}