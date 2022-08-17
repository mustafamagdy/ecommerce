using System.Data.Common;
using Ardalis.Specification;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Common.Extensions;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Infrastructure.Multitenancy;

internal class TenantService : ITenantService
{
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly ITenantUnitOfWork _uow;
  private readonly INonAggregateRepository<FSHTenantInfo> _repo;
  private readonly INonAggregateRepository<TenantProdSubscription> _nonAggregateProdSubscriptionRepo;
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
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;

  public TenantService(
    IMultiTenantStore<FSHTenantInfo> tenantStore,
    IConnectionStringSecurer csSecurer,
    IDatabaseInitializer dbInitializer,
    IJobService jobService,
    IMailService mailService,
    IEmailTemplateService templateService,
    IStringLocalizer<TenantService> localizer,
    IReadRepository<Branch> branchRepo,
    ITenantConnectionStringBuilder cnBuilder, IHostEnvironment env,
    ISystemTime systemTime,
    IReadRepository<PaymentMethod> paymentMethodRepo,
    ITenantUnitOfWork uow,
    INonAggregateRepository<FSHTenantInfo> repo,
    INonAggregateRepository<TenantProdSubscription> nonAggregateProdSubscriptionRepo)
  {
    _tenantStore = tenantStore;
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
    _paymentMethodRepo = paymentMethodRepo;
    _uow = uow;
    _repo = repo;
    _nonAggregateProdSubscriptionRepo = nonAggregateProdSubscriptionRepo;
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
    var prodSubscription = await TryCreateProdSubscription(tenant);

    bool result = await _uow.CommitAsync(cancellationToken) > 0;
    if (!result)
    {
      throw new DbUpdateException($"Failed to create tenant & subscription for {tenant.Name}");
    }

    try
    {
      await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);

      SendWelcomeEmail(tenant, request, prodSubscription);
    }
    catch
    {
      await _tenantStore.TryRemoveAsync(request.Id);
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

  private async Task<ProdTenantSubscriptionDto> TryCreateProdSubscription(FSHTenantInfo tenant)
  {
    var prodSubscription = await GetSubscription<StandardSubscription>(SubscriptionType.Standard);

    var today = _systemTime.Now;
    var tenantProdSubscription = new TenantProdSubscription(prodSubscription, tenant);
    tenantProdSubscription.Renew(today);

    await _uow.Set<TenantProdSubscription>().AddAsync(tenantProdSubscription);

    tenant.SetProdSubscription(tenantProdSubscription);
    return tenant.ProdSubscription.Adapt<ProdTenantSubscriptionDto>();
  }

  public async Task<Unit> PayForSubscription(Guid subscriptionId, decimal amount, Guid? paymentMethodId)
  {
    paymentMethodId ??= await GetCashPaymentMethod();
    var subscription = await _nonAggregateProdSubscriptionRepo.GetByIdAsync(subscriptionId);
    subscription.Pay(amount, paymentMethodId.Value);

    await _uow.CommitAsync();
    return Unit.Value;
  }

  private async Task<Guid> GetCashPaymentMethod()
  {
    var spec = new GetDefaultCashPaymentMethodSpec();
    var pm = await _paymentMethodRepo.FirstOrDefaultAsync(spec)
             ?? throw new ArgumentOutOfRangeException($"No default cash payment method register");
    return pm.Id;
  }

  public async Task<T> GetSubscription<T>(SubscriptionType subscriptionType)
    where T : Subscription
  {
    return subscriptionType.Name switch
    {
      nameof(SubscriptionType.Standard) => await _uow.Set<StandardSubscription>().FirstOrDefaultAsync() as T
                                           ?? throw new NotFoundException("No standard subscription found"),
      nameof(SubscriptionType.Demo) => await _uow.Set<DemoSubscription>().FirstOrDefaultAsync() as T
                                       ?? throw new NotFoundException("No demo subscription found"),
      nameof(SubscriptionType.Train) => await _uow.Set<TrainSubscription>().FirstOrDefaultAsync() as T
                                        ?? throw new NotFoundException("No train subscription found"),
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public async Task<bool> DatabaseExistAsync(string databaseName)
  {
    return (await _tenantStore.GetAllAsync()).Any(t =>
    {
      if (string.IsNullOrEmpty(t.ConnectionString)) return false;
      var cnBuilder = new DbConnectionStringBuilder
      {
        ConnectionString = t.ConnectionString
      };

      var newDbName = $"{_env.GetShortenName()}-{databaseName}";
      return cnBuilder.TryGetValue("initial catalog", out var dbName) && newDbName.Equals(dbName);
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

  public async Task<FSHTenantInfo> GetTenantById(string id)
  {
    var spec = new SingleResultSpecification<FSHTenantInfo>()
      .Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History)
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.Payments)
      .Include(a => a.DemoSubscription)
      .Include(a => a.TrainSubscription)
      .Where(a => a.Id == id);

    var tenant = await _repo.FirstOrDefaultAsync(spec.Specification)
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), id]);
    //
    //
    // var tenant = await _tenantDbContext.TenantInfo
    //                .Include(a => a.ProdSubscription)
    //                .ThenInclude(a => a.History)
    //                .Include(a => a.ProdSubscription)
    //                .ThenInclude(a => a.Payments)
    //                .Include(a => a.DemoSubscription)
    //                .Include(a => a.TrainSubscription)
    //                .Include(a => a.Branches)
    //                .FirstOrDefaultAsync(a => a.Id == id)
    //              ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), id]);
    return tenant;
  }

  public Task<bool> HasAValidProdSubscription(string tenantId)
  {
    var today = _systemTime.Now;

    var spec = new SingleResultSpecification<FSHTenantInfo>()
      .Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History)
      .Where(a => a.Id == tenantId
                  && a.Active
                  && a.ProdSubscription != null
                  && a.ProdSubscription.ExpiryDate >= today);

    return _repo.AnyAsync(spec.Specification);
  }
}