using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Operation.Orders;

public interface ICreateOrderHelper : ITransientService
{
  Task<Order> CreateCashOrder(IEnumerable<OrderItemRequest> items, Customer customer, Guid cashPaymentMethodId, CancellationToken cancellationToken);
  Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer, List<OrderPaymentAmount> payments, CancellationToken cancellationToken);
}

public class CreateOrderHelper : ICreateOrderHelper
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;
  private readonly IVatQrCodeGenerator _barcodeGenerator;
  private readonly IVatSettingProvider _vatSettingProvider;

  public CreateOrderHelper(IRepositoryWithEvents<Order> repository, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator, IVatQrCodeGenerator barcodeGenerator,
    IVatSettingProvider vatSettingProvider)
  {
    _repository = repository;
    _serviceCatalogRepo = serviceCatalogRepo;
    _sequenceGenerator = sequenceGenerator;
    _barcodeGenerator = barcodeGenerator;
    _vatSettingProvider = vatSettingProvider;
  }

  public Task<Order> CreateCashOrder(IEnumerable<OrderItemRequest> items, Customer customer, Guid cashPaymentMethodId, CancellationToken cancellationToken)
  {
    return CreateOrder(items, customer, new List<OrderPaymentAmount> { new() { PaymentMethodId = cashPaymentMethodId, Amount = 0 } }, true, cancellationToken);
  }

  public Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer, List<OrderPaymentAmount> payments, CancellationToken cancellationToken)
  {
    return CreateOrder(items, customer, payments, false, cancellationToken);
  }

  private async Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer,
    List<OrderPaymentAmount> payments, bool cashOrder, CancellationToken cancellationToken)
  {
    var orderItems = new List<OrderItem>();
    foreach (var item in items)
    {
      var serviceItem = await _serviceCatalogRepo.GetBySpecAsync((ISpecification<ServiceCatalog, ServiceCatalogDto>)new GetServiceCatalogDetailByIdSpec(item.ItemId),
        cancellationToken);
      if (serviceItem is null)
      {
        throw new ArgumentNullException(nameof(serviceItem));
      }

      orderItems.Add(new OrderItem(serviceItem.ServiceName, serviceItem.ProductName, item.ItemId, item.Qty,
        serviceItem.Price, TEMPHelper.VatPercent(), Guid.Empty));
    }

    decimal orderItemsTotal = orderItems.Sum(a => a.ItemTotal);
    decimal totalPaid = cashOrder ? orderItemsTotal : payments.Sum(a => a.Amount);
    if (totalPaid < orderItemsTotal)
    {
      throw new InvalidOperationException($"Total paid amount {totalPaid} doesn't match order net amount {orderItemsTotal}");
    }

    string orderNumber = await _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(customer.Id, orderNumber, DateTime.Now);
    order.OrderItems = orderItems.Select(a => a.SetOrderId(order.Id)).ToHashSet();

    var barcodeInfo = new KsaInvoiceBarcodeInfoInfo(
      _vatSettingProvider.LegalEntityName,
      _vatSettingProvider.VatRegNo,
      order.OrderDate,
      order.TotalAmount,
      order.TotalVat);

    string invoiceQrCode = _barcodeGenerator.ToBase64(barcodeInfo);
    order.SetInvoiceQrCode(invoiceQrCode);

    foreach (var payment in payments)
    {
      var cashPayment = new OrderPayment(order.Id, payment.PaymentMethodId, order.NetAmount);
      order.OrderPayments.Add(cashPayment);
    }

    await _repository.AddAsync(order, cancellationToken);
    return order;
  }
}