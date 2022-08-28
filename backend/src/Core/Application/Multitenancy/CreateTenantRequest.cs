using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace FSH.WebApi.Application.Multitenancy;

public class CreateTenantRequest : IRequest<BasicTenantInfoDto>
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string AdminEmail { get; set; }
  public Guid? ProdPackageId { get; set; }
  public Guid? DemoPackageId { get; set; }
  public string? Issuer { get; set; }
  public string? PhoneNumber { get; set; }
  public string? VatNo { get; set; }
  public string? Email { get; set; }
  public string? Address { get; set; }
  public string? AdminName { get; set; }
  public string? AdminPhoneNumber { get; set; }
  public string? TechSupportUserId { get; set; }
}

public class CreateTenantRequestHandler : IRequestHandler<CreateTenantRequest, BasicTenantInfoDto>
{
  private readonly ITenantUnitOfWork _uow;
  private readonly IApplicationUnitOfWork _appUow;
  private readonly ITenantConnectionStringBuilder _cnBuilder;
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly IDatabaseInitializer _dbInitializer;
  private readonly ISystemTime _systemTime;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
  private readonly IRepository<Branch> _branchRepo;

  public CreateTenantRequestHandler(ITenantUnitOfWork uow, ITenantConnectionStringBuilder cnBuilder,
    IMultiTenantStore<FSHTenantInfo> tenantStore, IDatabaseInitializer dbInitializer, ISystemTime systemTime,
    IReadNonAggregateRepository<FSHTenantInfo> tenantRepo, IRepository<Branch> branchRepo, IApplicationUnitOfWork appUow)
  {
    _uow = uow;
    _cnBuilder = cnBuilder;
    _tenantStore = tenantStore;
    _dbInitializer = dbInitializer;
    _systemTime = systemTime;
    _tenantRepo = tenantRepo;
    _branchRepo = branchRepo;
    _appUow = appUow;
  }

  public async Task<BasicTenantInfoDto> Handle(CreateTenantRequest request, CancellationToken cancellationToken)
  {
    string prodConnectionString = string.Empty;
    string trainConnectionString = string.Empty;
    if (request.ProdPackageId != null)
    {
      prodConnectionString = _cnBuilder.BuildConnectionString(request.Id, SubscriptionType.Standard);
      trainConnectionString = _cnBuilder.BuildConnectionString(request.Id, SubscriptionType.Train);
    }

    string demoConnectionString = request.DemoPackageId != null ? _cnBuilder.BuildConnectionString(request.Id, SubscriptionType.Demo) : string.Empty;

    var tenant = new FSHTenantInfo(request.Id, request.Name, prodConnectionString, demoConnectionString, trainConnectionString,
      request.AdminEmail, request.PhoneNumber, request.VatNo, request.Email, request.Address, request.AdminName, request.AdminPhoneNumber,
      request.TechSupportUserId, request.Issuer);

    await _tenantStore.TryAddAsync(tenant);

    var prodSubscription = await TryCreateProdSubscription(tenant);

    bool result = await _uow.CommitAsync(cancellationToken) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant & subscription for {tenant.Name}");
    }

    tenant.AddDomainEvent(new TenantCreatedEvent(tenant, prodSubscription));
    try
    {
      await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);

      await _appUow.CommitAsync(cancellationToken);
    }
    catch
    {
      await _tenantStore.TryRemoveAsync(request.Id);
      throw;
    }

    var tenantDto = await _tenantRepo.FirstOrDefaultAsync(new GetTenantBasicInfoSpec(tenant.Id), cancellationToken);
    return tenantDto;
  }

  private async Task<TenantProdSubscription> TryCreateProdSubscription(FSHTenantInfo tenant)
  {
    // var prodSubscription = await GetSubscription<StandardSubscription>(SubscriptionType.Standard);
    var prodSubscription = await GetDefaultMonthlyPackage();

    var today = _systemTime.Now;
    var tenantProdSubscription = new TenantProdSubscription(prodSubscription, tenant);
    tenantProdSubscription.Renew(today);

    await _uow.Set<TenantProdSubscription>().AddAsync(tenantProdSubscription);

    tenant.SetProdSubscription(tenantProdSubscription);
    return tenant.ProdSubscription;
  }

  private Task<SubscriptionPackage?> GetDefaultMonthlyPackage() =>
    _uow.Set<SubscriptionPackage>().FirstOrDefaultAsync(a => a.Default)
    ?? throw new InvalidOperationException("No default subscription package configured");

  // public async Task<T> GetSubscription<T>(SubscriptionType subscriptionType)
  //   where T : Subscription
  // {
  //   return subscriptionType.Name switch
  //   {
  //     nameof(SubscriptionType.Standard) => await _uow.Set<StandardSubscription>().FirstOrDefaultAsync() as T
  //                                          ?? throw new NotFoundException("No standard subscription found"),
  //     nameof(SubscriptionType.Demo) => await _uow.Set<DemoSubscription>().FirstOrDefaultAsync() as T
  //                                      ?? throw new NotFoundException("No demo subscription found"),
  //     nameof(SubscriptionType.Train) => await _uow.Set<TrainSubscription>().FirstOrDefaultAsync() as T
  //                                       ?? throw new NotFoundException("No train subscription found"),
  //     _ => throw new ArgumentOutOfRangeException()
  //   };
  // }
}