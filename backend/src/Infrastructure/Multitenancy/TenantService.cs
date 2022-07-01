using System.Data.Common;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class DemoService : IHostedService
{
  private readonly IJobService _jobService;

  public DemoService(IJobService jobService)
  {
    _jobService = jobService;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _jobService.Enqueue(() => Console.WriteLine("This is a demo job is running at startup, it can be anything"));
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    Console.WriteLine("Stopping async ");
    return Task.CompletedTask;
  }
}

internal class TenantService : ITenantService
{
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly TenantDbContext _tenantDbContext;
  private readonly IConnectionStringSecurer _csSecurer;
  private readonly IDatabaseInitializer _dbInitializer;
  private readonly IJobService _jobService;
  private readonly IMailService _mailService;
  private readonly IEmailTemplateService _templateService;
  private readonly IStringLocalizer _t;
  private readonly IReadRepository<Branch> _branchRepo;
  private readonly ITenantConnectionStringBuilder _cnBuilder;
  private readonly IHostEnvironment _env;

  public TenantService(
    IMultiTenantStore<FSHTenantInfo> tenantStore,
    TenantDbContext tenantDbContext,
    IConnectionStringSecurer csSecurer,
    IDatabaseInitializer dbInitializer,
    IJobService jobService,
    IMailService mailService,
    IEmailTemplateService templateService,
    IStringLocalizer<TenantService> localizer,
    IReadRepository<Branch> branchRepo,
    ITenantConnectionStringBuilder cnBuilder, IHostEnvironment env)
  {
    _tenantStore = tenantStore;
    _tenantDbContext = tenantDbContext;
    _csSecurer = csSecurer;
    _dbInitializer = dbInitializer;
    _jobService = jobService;
    _mailService = mailService;
    _templateService = templateService;
    _t = localizer;
    _branchRepo = branchRepo;
    _cnBuilder = cnBuilder;
    _env = env;
  }

  public async Task<List<TenantDto>> GetAllAsync()
  {
    var tenants = (await _tenantStore.GetAllAsync()).Adapt<List<TenantDto>>();
    // tenants.ForEach(t => t.DatabaseName = _csSecurer.MakeSecure(t.DatabaseName));
    return tenants;
  }

  public async Task<bool> ExistsWithIdAsync(string id) =>
    await _tenantStore.TryGetAsync(id) is not null;

  public async Task<bool> ExistsWithNameAsync(string name) =>
    (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);

  public async Task<TenantDto> GetByIdAsync(string id)
  {
    var tenant = await GetTenantInfoAsync(id);
    return tenant.Adapt<TenantDto>();
  }

  public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
  {
    string connectionString = string.IsNullOrWhiteSpace(request.DatabaseName) ? string.Empty : _cnBuilder.BuildConnectionString(request.DatabaseName);

    var tenant = new FSHTenantInfo(request.Id, request.Name, connectionString, request.AdminEmail,
      request.PhoneNumber, request.VatNo, request.Email, request.Address, request.AdminName, request.AdminPhoneNumber,
      request.TechSupportUserId, request.Issuer);

    await _tenantStore.TryAddAsync(tenant);
    var subscription = await TryCreateMonthlySubscription(tenant);
    try
    {
      await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);

      SendWelcomeEmail(tenant, request, subscription);
    }
    catch
    {
      await _tenantStore.TryRemoveAsync(request.Id);
      await TryRemoveSubscriptions(tenant.Id);
      throw;
    }

    if (request.CreateDemoSubscription == true)
    {
      await CreateDemoTenantWithSubscription(request, tenant, cancellationToken);
    }

    return tenant.Id;
  }

  private async Task CreateDemoTenantWithSubscription(CreateTenantRequest request, FSHTenantInfo tenant, CancellationToken cancellationToken)
  {
    //TODO: need to relate demo account to the original tenant,
    var demoTenantName = tenant.Key + "_demo";
    var demoSubscription = await CreateDemoSubscription(tenant, cancellationToken);
    SendWelcomeEmail(tenant, request, demoSubscription);
  }

  private async Task<TenantSubscriptionInfo> CreateDemoSubscription(FSHTenantInfo tenant, CancellationToken cancellationToken)
  {
    var subscription = await GetDefaultMonthlySubscription();

    var today = DateTime.Now;
    var newExpiryDate = today.AddDays(subscription.Days);
    var tenantSubscription = new TenantSubscription(tenant.Id, subscription.Id, today, subscription.Price, false);
    tenantSubscription.Extend(newExpiryDate);

    await _tenantDbContext.AddAsync(tenantSubscription, cancellationToken);
    bool result = (await _tenantDbContext.SaveChangesAsync(cancellationToken)) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant subscription for {tenant.Name}");
    }

    return tenantSubscription.Adapt<TenantSubscriptionInfo>();
  }

  private void SendWelcomeEmail(FSHTenantInfo tenant, CreateTenantRequest request, TenantSubscriptionInfo subscription)
  {
    string demoUrl = $"https://demo.abcd.com/{tenant.Key}";
    string prodUrl = $"https://prod.abcd.com/{tenant.Key}";

    var eMailModel = new TenantCreatedEmailModel()
    {
      AdminEmail = request.AdminEmail,
      TenantName = request.Name,
      SubscriptionExpiryDate = subscription.ExpiryDate,
      SiteUrl = subscription.IsDemo ? demoUrl : prodUrl
    };

    var mailRequest = new MailRequest(
      new List<string> { request.AdminEmail },
      _t["Subscription Created"],
      _templateService.GenerateEmailTemplate("email-subscription", eMailModel));

    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
  }

  private async Task<TenantSubscriptionInfo> TryCreateMonthlySubscription(FSHTenantInfo tenant)
  {
    var subscription = await GetDefaultMonthlySubscription();

    var today = DateTime.Now;
    var newExpiryDate = today.AddDays(subscription.Days);
    var tenantSubscription = new TenantSubscription(tenant.Id, subscription.Id, today, subscription.Price, false);
    tenantSubscription.Extend(newExpiryDate);

    await _tenantDbContext.AddAsync(tenantSubscription);
    bool result = (await _tenantDbContext.SaveChangesAsync()) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant subscription for {tenant.Name}");
    }

    return tenantSubscription.Adapt<TenantSubscriptionInfo>();
  }

  private Task<Subscription> GetDefaultMonthlySubscription()
  {
    return (_tenantDbContext.Subscriptions.FirstOrDefaultAsync(a => a.DefaultMonthly)
            ?? throw new NotImplementedException("There is no default subscription"))!;
  }

  private async Task TryRemoveSubscriptions(string tenantId)
  {
    var subscriptions = await _tenantDbContext.TenantSubscriptions.Where(a => a.TenantId == tenantId).ToArrayAsync();
    if (subscriptions.Length > 0)
    {
      _tenantDbContext.RemoveRange(subscriptions);
      await _tenantDbContext.SaveChangesAsync();
    }
  }

  public async Task<string> ActivateAsync(string tenantId)
  {
    var tenant = await GetTenantInfoAsync(tenantId);

    if (tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Activated."]);
    }

    tenant.Activate();

    await _tenantStore.TryUpdateAsync(tenant);

    var newExpiryDate = DateTime.Now.AddMonths(1);
    var activeSubscriptions = await GetActiveSubscriptions(tenantId);
    if (activeSubscriptions.Count > 0)
    {
      var activeSubscription = activeSubscriptions.First();
      activeSubscription.Extend(newExpiryDate);

      _tenantDbContext.Update(activeSubscription);
    }
    else
    {
      await TryCreateMonthlySubscription(tenant);
    }

    return _t["Tenant {0} is now Activated.", tenantId];
  }

  public async Task<string> DeactivateAsync(string tenantId)
  {
    var tenant = await GetTenantInfoAsync(tenantId);

    if (!tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Deactivated."]);
    }

    tenant.Deactivate();

    await _tenantStore.TryUpdateAsync(tenant);

    var activeSubscriptions = await GetActiveSubscriptions(tenantId);
    if (activeSubscriptions.Count > 0)
    {
      var activeSubscription = activeSubscriptions.First();
      activeSubscription.DeActivate();

      _tenantDbContext.Update(activeSubscription);
    }

    return _t[$"Tenant {0} is now Deactivated.", tenantId];
  }

  public async Task<string> RenewSubscription(Guid subscriptionId, DateTime? extendedExpiryDate)
  {
    var tenantSubscription = await _tenantDbContext.TenantSubscriptions.FindAsync(subscriptionId);
    if (tenantSubscription == null)
    {
      throw new NotFoundException(_t["Subscription not found."]);
    }

    if (extendedExpiryDate == null)
    {
      var today = DateTime.Now;
      var subscription =
        await _tenantDbContext.Subscriptions.FirstOrDefaultAsync(a => a.Id == tenantSubscription.SubscriptionId);
      if (subscription == null)
      {
        throw new NotFoundException(_t["Subscription {0} not found", tenantSubscription.SubscriptionId]);
      }

      extendedExpiryDate = today.AddDays(subscription.Days);
    }

    tenantSubscription.Extend(extendedExpiryDate.Value);
    _tenantDbContext.Update(tenantSubscription);
    await _tenantDbContext.SaveChangesAsync();

    return _t["Subscription {0} renewed. Now Valid till {1}.", tenantSubscription.Id, tenantSubscription.ExpiryDate];
  }

  public async Task<bool> HasAValidSubscription(string tenantId) => (await GetActiveSubscriptions(tenantId)).Count > 0;

  public async Task<IReadOnlyList<TenantSubscription>> GetActiveSubscriptions(string tenantId)
  {
    var now = DateTime.Now;
    return (await _tenantDbContext.TenantSubscriptions
        .Where(a => a.TenantId == tenantId && a.ExpiryDate >= now && a.StartDate <= now && a.Active && !a.IsDemo)
        .ToListAsync()
      ).AsReadOnly();
  }

  public Task<List<TenantSubscription>> GetAllTenantSubscriptions(string tenantId)
    => _tenantDbContext.TenantSubscriptions.Where(a => a.TenantId == tenantId).ToListAsync();

  public async Task<bool> DatabaseExistAsync(string databaseName)
  {
    return (await _tenantStore.GetAllAsync()).Any(t =>
    {
      if (string.IsNullOrEmpty(t.ConnectionString)) return false;
      var cnBuilder = new DbConnectionStringBuilder();
      cnBuilder.ConnectionString = t.ConnectionString;

      var _dbName = $"{_env.GetShortenName()}-{databaseName}";
      return cnBuilder.TryGetValue("initial catalog", out var dbName) && _dbName.Equals(dbName);
    });
  }

  private async Task<FSHTenantInfo> GetTenantInfoAsync(string id)
  {
    var tenant = await _tenantStore.TryGetAsync(id)
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), id]);

    return tenant;
  }

  public async Task<BasicTenantInfoDto> GetBasicInfoByIdAsync(string id)
  {
    var tenant = await _tenantStore.TryGetAsync(id)
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), id]);

    var tenantBranchSpec = new TenantBranchSpec(id);
    var branches = await _branchRepo.ListAsync(tenantBranchSpec);

    var tenantDto = tenant.Adapt<BasicTenantInfoDto>();
    tenantDto.Branches = branches.Adapt<List<BranchDto>>();

    var activeSubscription = (await GetActiveSubscriptions(id)).FirstOrDefault(a => !a.IsDemo);
    tenantDto.CurrentSubscription = activeSubscription?.Adapt<BasicSubscriptionInfoDto>();

    return tenantDto;
  }
}