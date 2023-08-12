// using AutoFixture;
// using Finbuckle.MultiTenant;
// using FSH.WebApi.Application.Common.Interfaces;
// using FSH.WebApi.Application.Common.Persistence;
// using FSH.WebApi.Application.Multitenancy;
// using FSH.WebApi.Application.Operation.Orders;
// using FSH.WebApi.Application.Settings.Vat;
// using FSH.WebApi.Domain.Catalog;
// using FSH.WebApi.Domain.Operation;
// using FSH.WebApi.Infrastructure.Multitenancy;
// using FSH.WebApi.Infrastructure.Persistence;
// using FSH.WebApi.Infrastructure.Persistence.Context;
// using FSH.WebApi.Shared.Finance;
// using FSH.WebApi.Shared.Multitenancy;
// using FSH.WebApi.Shared.Persistence;
// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Localization;
// using Microsoft.Extensions.Options;
// using NSubstitute;
// using Xunit;
//
// namespace Application.Tests.Operations;
//
// public class OrderTests
// {
//   private readonly Fixture _fixture = new();
//   private IRepositoryWithEvents<Order> _repository;
//   private IReadRepository<ServiceCatalog> _serviceCatalogRepo;
//   private IRepositoryWithEvents<CashRegister> _cashRegisterRepo;
//   private ITenantSequenceGenerator _sequenceGenerator;
//   private IVatSettingProvider _vatSettingProvider;
//   private IStringLocalizer<CreateOrderHelper> _t;
//   private ISystemTime _systemTime;
//   private ICashRegisterResolver _cashRegisterResolver;
//   private IVatQrCodeGenerator _vatQrCodeGenerator;
//   private IHttpContextAccessor _httpContextAccessor;
//
//   private IApplicationUnitOfWork _uow;
//
//   private long i = 0;
//   private Guid _cashRegisterId = Guid.NewGuid();
//   private Guid _tenantId = Guid.NewGuid();
//
//   public OrderTests()
//   {
//     _sequenceGenerator = Substitute.For<ITenantSequenceGenerator>();
//     _sequenceGenerator.NextFormatted(Arg.Any<string>()).Returns(Task.FromResult((i++).ToString()));
//
//     _vatQrCodeGenerator = Substitute.For<IVatQrCodeGenerator>();
//     _vatQrCodeGenerator.ToBase64(Arg.Any<IInvoiceBarcodeInfo>()).Returns("base64 qr code");
//
//     _vatSettingProvider = Substitute.For<IVatSettingProvider>();
//     _vatSettingProvider.LegalEntityName.Returns("Test entity");
//     _vatSettingProvider.VatRegNo.Returns("1234567890");
//     _t = Substitute.For<IStringLocalizer<CreateOrderHelper>>();
//     _systemTime = Substitute.For<ISystemTime>();
//     _systemTime.Now.Returns(DateTime.Now);
//
//     _cashRegisterResolver = Substitute.For<ICashRegisterResolver>();
//     _cashRegisterResolver.Resolve(Arg.Any<object>()).Returns(_cashRegisterId);
//
//     _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
//     _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());
//
//     _cashRegisterRepo = Substitute.For<IRepositoryWithEvents<CashRegister>>();
//     _cashRegisterRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new CashRegister(Guid.Empty, "", ""));
//
//     _repository = Substitute.For<IRepositoryWithEvents<Order>>();
//   }
//
//   [Fact]
//   public async Task order_()
//   {
//     var tenant = Substitute.For<ITenantInfo>();
//     tenant.Id.Returns(_tenantId.ToString());
//     tenant.Identifier.Returns(_tenantId.ToString());
//     tenant.Name.Returns(_tenantId.ToString());
//     var tenantDbConnectionString = "Server=127.0.0.1;Port={0};Database={1};Uid=postgres;Pwd=DeV12345";
//     var dbName = Guid.NewGuid();
//     var port = 5432;
//     string cnnString = string.Format(tenantDbConnectionString, port, dbName);
//     tenant.ConnectionString.Returns(cnnString);
//
//     var subscription = Substitute.For<ISubscriptionInfo>();
//     subscription.SubscriptionType.Returns(SubscriptionType.Standard);
//
//     var dbOptBuilder = new DbContextOptionsBuilder();
//     var dbOpt = dbOptBuilder.Options;
//
//     var user = Substitute.For<ICurrentUser>();
//     user.Name.Returns("test");
//
//     var jsonService = Substitute.For<ISerializerService>();
//
//     var cnnBuilder = Substitute.For<ITenantConnectionStringBuilder>();
//     cnnBuilder.BuildConnectionString(Arg.Any<string>()).Returns(cnnString);
//
//     var opts = Substitute.For<IOptions<DatabaseSettings>>();
//     opts.Value.Returns(new DatabaseSettings()
//     {
//       DBProvider = "postgresql",
//       ConnectionString = cnnString,
//       LogSensitiveInfo = true,
//     });
//
//     var cnnResolver = Substitute.For<ITenantConnectionStringResolver>();
//     cnnResolver.Resolve(Arg.Any<string>(), Arg.Any<SubscriptionType>()).Returns(cnnString);
//
//     var db = new ApplicationDbContext(tenant, subscription, dbOpt, user, jsonService, cnnBuilder, opts, cnnResolver);
//     _uow = new ApplicationUnitOfWork(db);
//
//     var orderHelper = new CreateOrderHelper(_repository, _serviceCatalogRepo,
//       _sequenceGenerator, _vatQrCodeGenerator, _vatSettingProvider,
//       _t, _systemTime, _cashRegisterResolver, _httpContextAccessor, _uow, _cashRegisterRepo);
//
//     var items = new List<OrderItemRequest>();
//     var customer = _fixture.Create<Customer>();
//     var cashPaymentMethodId = Guid.NewGuid();
//     var result = await orderHelper.CreateCashOrder(items, customer, cashPaymentMethodId, CancellationToken.None);
//   }
// }