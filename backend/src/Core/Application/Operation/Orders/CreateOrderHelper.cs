using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Operation.Orders;

public interface ICreateOrderHelper : ITransientService
{
  Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer, PaymentMethod cashPaymentMethod,
    CancellationToken cancellationToken);
}

public class CreateOrderHelper : ICreateOrderHelper
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IRepositoryWithEvents<OrderPayment> _paymentRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;
  private readonly IInvoiceBarcodeGenerator _barcodeGenerator;
  private readonly IVatSettingProvider _vatSettingProvider;

  public CreateOrderHelper(IRepositoryWithEvents<Order> repository, IReadRepository<Customer> customerRepo,
    IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator, IReadRepository<PaymentMethod> paymentMethodRepo,
    IRepositoryWithEvents<OrderPayment> paymentRepo, IInvoiceBarcodeGenerator barcodeGenerator,
    IVatSettingProvider vatSettingProvider)
  {
  }

  public async Task<Order> CreateOrder(IEnumerable<OrderItemRequest> items, Customer customer,
    PaymentMethod cashPaymentMethod, CancellationToken cancellationToken)
  {
    var orderNumber = await _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(customer.Id, orderNumber, DateTime.Now);

    foreach (var item in items)
    {
      var serviceItem = await _serviceCatalogRepo.GetBySpecAsync(
        (ISpecification<ServiceCatalog, ServiceCatalogDto>)new GetServiceCatalogDetailByIdSpec(item.ItemId),
        cancellationToken);
      if (serviceItem is null)
      {
        throw new ArgumentNullException(nameof(serviceItem));
      }

      order.OrderItems.Add(new OrderItem(serviceItem.ServiceName, serviceItem.ProductName, item.ItemId, item.Qty,
        serviceItem.Price, TEMPHelper.VatPercent(), order.Id));
    }

    var barcodeInfo = new KsaInvoiceBarcodeInfoInfo(_vatSettingProvider.LegalEntityName,
      _vatSettingProvider.VatRegNo, order.OrderDate, order.TotalAmount, order.TotalVat);

    string invoiceQrCode = _barcodeGenerator.ToBase64(barcodeInfo);
    order.SetInvoiceQrCode(invoiceQrCode);

    var cashPayment = new OrderPayment(order.Id, cashPaymentMethod.Id, order.NetAmount);
    order.OrderPayments.Add(cashPayment);
    await _repository.AddAsync(order, cancellationToken);
    return order;
  }
}