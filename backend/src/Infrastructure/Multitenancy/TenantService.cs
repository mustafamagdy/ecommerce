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
using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Infrastructure.Multitenancy;

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
  private readonly ISystemTime _systemTime;

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
    ITenantConnectionStringBuilder cnBuilder, IHostEnvironment env, ISystemTime systemTime)
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
    _systemTime = systemTime;
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
    var tenant = await GetTenantById(id);
    return tenant.Adapt<TenantDto>();
  }

  public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
  {
    string connectionString = string.IsNullOrWhiteSpace(request.DatabaseName) ? string.Empty : _cnBuilder.BuildConnectionString(request.DatabaseName);

    var tenant = new FSHTenantInfo(request.Id, request.Name, connectionString, request.AdminEmail,
      request.PhoneNumber, request.VatNo, request.Email, request.Address, request.AdminName, request.AdminPhoneNumber,
      request.TechSupportUserId, request.Issuer);

    await _tenantStore.TryAddAsync(tenant);
    var subscription = await TryCreateSubscription<StandardSubscription, ProdTenantSubscriptionDto>(tenant, SubscriptionType.Standard);
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

    return tenant.Id;
  }

  private void SendWelcomeEmail(FSHTenantInfo tenant, CreateTenantRequest request, TenantSubscriptionDto subscription)
  {
    string prodUrl = $"https://prod.abcd.com/{tenant.Key}";

    var eMailModel = new TenantCreatedEmailModel()
    {
      AdminEmail = request.AdminEmail,
      TenantName = request.Name,
      SubscriptionExpiryDate = subscription.ExpiryDate,
      SiteUrl = prodUrl
    };

    var mailRequest = new MailRequest(
      new List<string> { request.AdminEmail },
      _t["Subscription Created"],
      _templateService.GenerateEmailTemplate("email-subscription", eMailModel));

    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
  }

  private async Task<TDto> TryCreateSubscription<T, TDto>(FSHTenantInfo tenant, SubscriptionType subscriptionType)
    where T : Subscription
    where TDto : TenantSubscriptionDto, new()
  {
    T subscription = await GetSubscription<T>(subscriptionType);
    tenant.ProdSubscriptionId = subscription.Id;

    var today = _systemTime.Now;
    var subHistory = new SubscriptionHistory(tenant.Id, subscription.Id, today, subscription.Days, subscription.Price);

    await _tenantDbContext.AddAsync(subHistory);
    bool result = (await _tenantDbContext.SaveChangesAsync()) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant subscription for {tenant.Name}");
    }

    var historyDto = subscription.Adapt<SubscriptionHistoryDto>();
    return new TDto
    {
      History = new List<SubscriptionHistoryDto> { historyDto },
      Id = subHistory.Id,
      ExpiryDate = historyDto.ExpireDate,
      TenantId = tenant.Id
    };
  }

  private async Task<T> GetSubscription<T>(SubscriptionType subscriptionType)
    where T : Subscription
  {
    return subscriptionType.Name switch
    {
      nameof(SubscriptionType.Standard) => await _tenantDbContext.StandardSubscriptions.SingleOrDefaultAsync() as T
                                           ?? throw new NotFoundException("No standard subscription found"),
      nameof(SubscriptionType.Demo) => await _tenantDbContext.DemoSubscriptions.SingleOrDefaultAsync() as T
                                       ?? throw new NotFoundException("No demo subscription found"),
      nameof(SubscriptionType.Train) => await _tenantDbContext.TrainSubscriptions.SingleOrDefaultAsync() as T
                                        ?? throw new NotFoundException("No train subscription found"),
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  private async Task<Subscription> GetDefaultMonthlySubscription()
  {
    return (await _tenantDbContext.StandardSubscriptions.SingleOrDefaultAsync()
            ?? throw new NotImplementedException("There is no standard subscription configured"))!;
  }

  private async Task TryRemoveSubscriptions(string tenantId)
  {
    var history = await _tenantDbContext.SubscriptionHistories.Where(a => a.TenantId == tenantId).ToArrayAsync();
    if (history.Length > 0)
    {
      _tenantDbContext.RemoveRange(history);
      await _tenantDbContext.SaveChangesAsync();
    }
  }

  public async Task<string> ActivateAsync(string tenantId)
  {
    var tenant = await GetTenantById(tenantId);

    if (tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Activated."]);
    }

    tenant.Activate();

    await _tenantStore.TryUpdateAsync(tenant);

    return _t["Tenant {0} is now Activated.", tenantId];
  }

  public async Task<string> DeactivateAsync(string tenantId)
  {
    var tenant = await GetTenantById(tenantId);

    if (!tenant.Active)
    {
      throw new ConflictException(_t["Tenant is already Deactivated."]);
    }

    tenant.DeActivate();

    await _tenantStore.TryUpdateAsync(tenant);

    return _t[$"Tenant {0} is now Deactivated.", tenantId];
  }

  public async Task<string> RenewSubscription(Guid subscriptionId, string tenantId, int? days = null)
  {
    var subRecord = await _tenantDbContext
      .SubscriptionHistories
      .Include(a => a.Subscription)
      .FirstOrDefaultAsync(a => a.SubscriptionId == subscriptionId && a.TenantId == tenantId);

    if (subRecord == null)
    {
      throw new NotFoundException(_t["Subscription not found."]);
    }

    var today = _systemTime.Now;
    var newHistoryRecord = new SubscriptionHistory(subRecord.TenantId,
      subscriptionId,
      today,
      days ?? subRecord.Subscription.Days,
      subRecord.Price);

    await _tenantDbContext.SubscriptionHistories.AddAsync(newHistoryRecord);
    await _tenantDbContext.SaveChangesAsync();

    return _t["Subscription {0} renewed. Now Valid till {1}.", subRecord.Id, subRecord.ExpiryDate];
  }

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

  public async Task<BasicTenantInfoDto> GetBasicInfoByIdAsync(string id)
  {
    var tenant = await GetTenantById(id);

    var tenantBranchSpec = new TenantBranchSpec(id);
    var branches = await _branchRepo.ListAsync(tenantBranchSpec);

    var tenantDto = tenant.Adapt<BasicTenantInfoDto>();
    tenantDto.Branches = branches.Adapt<List<BranchDto>>();

    return tenantDto;
  }

  private async Task<FSHTenantInfo> GetTenantById(string id)
  {
    var tenant = await _tenantDbContext.TenantInfo
                   .Include(a => a.ProdSubscription)
                   .ThenInclude(a => a.SubscriptionHistory)
                   .Include(a => a.DemoSubscription)
                   .Include(a => a.TrainSubscription)
                   .Include(a => a.Branches)
                   .FirstOrDefaultAsync(a => a.Id == id)
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), id]);
    return tenant;
  }

  public Task<bool> HasAValidProdSubscription(string tenantId)
  {
    var today = _systemTime.Now;
    return _tenantDbContext.TenantInfo
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.SubscriptionHistory)
      .AnyAsync(a => a.Id == tenantId
                     && a.Active
                     && a.ProdSubscription != null
                     && a.ProdSubscription.SubscriptionHistory.Any(x => x.TenantId == tenantId && x.ExpiryDate >= today));
  }
}