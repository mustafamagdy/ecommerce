using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
  private readonly UserManager<ApplicationUser> _userManager;

  public CreateTenantRequestHandler(ITenantUnitOfWork uow, ITenantConnectionStringBuilder cnBuilder,
    IMultiTenantStore<FSHTenantInfo> tenantStore, IDatabaseInitializer dbInitializer, ISystemTime systemTime,
    IReadNonAggregateRepository<FSHTenantInfo> tenantRepo, IRepository<Branch> branchRepo, IApplicationUnitOfWork appUow, UserManager<ApplicationUser> userManager)
  {
    _uow = uow;
    _cnBuilder = cnBuilder;
    _tenantStore = tenantStore;
    _dbInitializer = dbInitializer;
    _systemTime = systemTime;
    _tenantRepo = tenantRepo;
    _branchRepo = branchRepo;
    _appUow = appUow;
    _userManager = userManager;
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

    var supportEng = await _userManager.FindByIdAsync(request.TechSupportUserId)
                     ?? throw new NotFoundException($"Support user {request.TechSupportUserId} not found");

    var tenant = new FSHTenantInfo(request.Id, request.Name, prodConnectionString, demoConnectionString, trainConnectionString,
      request.AdminEmail, request.PhoneNumber, request.VatNo, request.Email, request.Address, request.AdminName, request.AdminPhoneNumber,
      supportEng.Id, request.Issuer);

    await _tenantStore.TryAddAsync(tenant);

    var subscriptions = await CreateTenantSubscriptions(tenant, request);

    bool result = await _uow.CommitAsync(cancellationToken) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant & subscription for {tenant.Name}");
    }

    tenant.AddDomainEvent(new TenantCreatedEvent(tenant, subscriptions));

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

  private async Task<List<TenantSubscription>> CreateTenantSubscriptions(FSHTenantInfo tenant, CreateTenantRequest request)
  {
    var list = new List<TenantSubscription>();
    if (request.ProdPackageId != null)
    {
      list.Add(await TryCreateSubscription(tenant, SubscriptionType.Standard, request.ProdPackageId));
      list.Add(await TryCreateSubscription(tenant, SubscriptionType.Train, request.ProdPackageId));
    }

    if (request.DemoPackageId != null)
    {
      list.Add(await TryCreateSubscription(tenant, SubscriptionType.Demo, request.DemoPackageId));
    }

    return list;
  }

  private async Task<TenantSubscription> TryCreateSubscription(FSHTenantInfo tenant, SubscriptionType subscriptionType, Guid? packageId)
  {
    var package = await GetPackageOrDefaultPackage(packageId);

    var today = _systemTime.Now;

    switch (subscriptionType.Name)
    {
      case nameof(SubscriptionType.Standard):
        var prodSubscription = new TenantProdSubscription(package, tenant);
        prodSubscription.Renew(today);
        await _uow.Set<TenantProdSubscription>().AddAsync(prodSubscription);
        tenant.SetProdSubscription(prodSubscription);
        return prodSubscription;
      case nameof(SubscriptionType.Demo):
        var demoSubscription = new TenantDemoSubscription(package, tenant);
        demoSubscription.Renew(today);
        await _uow.Set<TenantDemoSubscription>().AddAsync(demoSubscription);
        tenant.SetDemoSubscription(demoSubscription);
        return demoSubscription;
      case nameof(SubscriptionType.Train):
        var trainSubscription = new TenantTrainSubscription(package, tenant);
        trainSubscription.Renew(today);
        await _uow.Set<TenantTrainSubscription>().AddAsync(trainSubscription);
        tenant.SetTrainSubscription(trainSubscription);
        return trainSubscription;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private Task<SubscriptionPackage?> GetPackageOrDefaultPackage(Guid? packageId) =>
    packageId == null
      ? _uow.Set<SubscriptionPackage>().FirstOrDefaultAsync(a => a.Default)
        ?? throw new InvalidOperationException("No default subscription package configured")
      : _uow.Set<SubscriptionPackage>().FirstOrDefaultAsync(a => a.Id == packageId);
}