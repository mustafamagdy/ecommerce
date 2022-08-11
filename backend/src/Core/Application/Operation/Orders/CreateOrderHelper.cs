using System.Diagnostics;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Finance;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Application.Operation.Orders;

public interface ICreateOrderHelper : ITransientService
{
  Task<Order> CreateCashOrder(IEnumerable<OrderItemRequest> items, Customer customer, Guid cashPaymentMethodId, CancellationToken cancellationToken);
  Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer, List<OrderPaymentAmount> payments, CancellationToken cancellationToken);
}

public class GetServiceCatalogDetailByIdSpec : Specification<ServiceCatalog, ServiceCatalogDto>, ISingleResultSpecification
{
  public GetServiceCatalogDetailByIdSpec(Guid serviceCatalogId) =>
    Query
      .Include(a => a.Product)
      .Include(a => a.Service)
      .Where(a => a.Id == serviceCatalogId);
}

public class CreateOrderHelper : ICreateOrderHelper
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepositoryWithEvents<CashRegister> _cashRegisterRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;
  private readonly IVatQrCodeGenerator _barcodeGenerator;
  private readonly IVatSettingProvider _vatSettingProvider;
  private readonly IStringLocalizer _t;
  private readonly ISystemTime _systemTime;
  private readonly ICashRegisterResolver _cashRegisterResolver;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IApplicationUnitOfWork _uow;

  public CreateOrderHelper(IRepositoryWithEvents<Order> repository, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator, IVatQrCodeGenerator barcodeGenerator,
    IVatSettingProvider vatSettingProvider, IStringLocalizer<CreateOrderHelper> localizer, ISystemTime systemTime,
    ICashRegisterResolver cashRegisterResolver, IHttpContextAccessor httpContextAccessor,
    IApplicationUnitOfWork uow, IRepositoryWithEvents<CashRegister> cashRegisterRepo)
  {
    _repository = repository;
    _serviceCatalogRepo = serviceCatalogRepo;
    _sequenceGenerator = sequenceGenerator;
    _barcodeGenerator = barcodeGenerator;
    _vatSettingProvider = vatSettingProvider;
    _t = localizer;
    _systemTime = systemTime;
    _cashRegisterResolver = cashRegisterResolver;
    _httpContextAccessor = httpContextAccessor;
    _uow = uow;
    _cashRegisterRepo = cashRegisterRepo;
  }

  public Task<Order> CreateCashOrder(IEnumerable<OrderItemRequest> items, Customer customer, Guid cashPaymentMethodId, CancellationToken cancellationToken)
    => CreateOrder(items, customer, new List<OrderPaymentAmount> { new() { PaymentMethodId = cashPaymentMethodId, Amount = 0 } }, cashOrder: true, cancellationToken);

  public Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer, List<OrderPaymentAmount> payments, CancellationToken cancellationToken)
    => CreateOrder(items, customer, payments, cashOrder: false, cancellationToken);

  private async Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer,
    List<OrderPaymentAmount> payments, bool cashOrder, CancellationToken cancellationToken)
  {
    var orderItems = GetOrderItems(items, cancellationToken);

    if (cashOrder)
    {
      Debug.Assert(payments is { Count: 1 }, "Cash orders should have only one payment item");
      payments[0].Amount = orderItems.Sum(a => a.ItemTotal);
    }

    decimal orderItemsTotal = orderItems.Sum(a => a.ItemTotal);
    decimal totalPaid = payments.Sum(a => a.Amount);

    if (totalPaid < orderItemsTotal)
    {
      throw new InvalidOperationException(_t["Total paid amount {0} doesn't match order net amount {1}", totalPaid, orderItemsTotal]);
    }

    string orderNumber = await _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(customer.Id, orderNumber, _systemTime.Now);

    order.AddItems(orderItems);

    var barcodeInfo = new KsaInvoiceBarcodeInfoInfo(
      _vatSettingProvider.LegalEntityName,
      _vatSettingProvider.VatRegNo,
      order.OrderDate,
      order.TotalAmount,
      order.TotalVat);

    string invoiceQrCode = _barcodeGenerator.ToBase64(barcodeInfo);
    order.SetInvoiceQrCode(invoiceQrCode);

    var cashRegisterId = await _cashRegisterResolver.Resolve(_httpContextAccessor.HttpContext);
    var cashRegister = await _cashRegisterRepo.GetByIdAsync(cashRegisterId, cancellationToken);
    foreach (var payment in payments)
    {
      cashRegister.AddOperation(new ActivePaymentOperation(cashRegister, payment.PaymentMethodId, payment.Amount, _systemTime.Now, PaymentOperationType.In));
      var cashPayment = new OrderPayment(order.Id, payment.PaymentMethodId, payment.Amount);
      order.AddPayment(cashPayment);
    }

    await _repository.AddAsync(order, cancellationToken);

    await _uow.CommitAsync(cancellationToken);
    return order;
  }

  private List<OrderItem> GetOrderItems(IEnumerable<OrderItemRequest> items, CancellationToken cancellationToken)
  {
    return items.Select(async item =>
      {
        var serviceItem = await _serviceCatalogRepo.FirstOrDefaultAsync(new GetServiceCatalogDetailByIdSpec(item.ItemId), cancellationToken);
        return new OrderItem(item.ItemId, item.Qty, serviceItem.Price, serviceItem.ProductName, serviceItem.ServiceName, TEMPHelper.VatPercent());
      })
      .Select(a => a.Result)
      .ToList();
  }
}