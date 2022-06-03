using System.Diagnostics;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateCashOrderRequest : BaseOrderRequest, IRequest<OrderDto>
{
}

public class CreateCashOrderRequestValidator : CreateOrderRequestBaseValidator<CreateCashOrderRequest>
{
  public CreateCashOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
  }
}

public class CreateCashOrderRequestHandler : IRequestHandler<CreateCashOrderRequest, OrderDto>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IRepositoryWithEvents<OrderPayment> _paymentRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;
  private readonly IInvoiceBarcodeGenerator _barcodeGenerator;
  private readonly IVatSettingProvider _vatSettingProvider;

  public CreateCashOrderRequestHandler(IRepositoryWithEvents<Order> repository, IReadRepository<Customer> customerRepo,
    IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator, IReadRepository<PaymentMethod> paymentMethodRepo,
    IRepositoryWithEvents<OrderPayment> paymentRepo, IInvoiceBarcodeGenerator barcodeGenerator,
    IVatSettingProvider vatSettingProvider)
  {
    _repository = repository;
    _customerRepo = customerRepo;
    _orderItemRepo = orderItemRepo;
    _serviceCatalogRepo = serviceCatalogRepo;
    _sequenceGenerator = sequenceGenerator;
    _paymentMethodRepo = paymentMethodRepo;
    _paymentRepo = paymentRepo;
    _barcodeGenerator = barcodeGenerator;
    _vatSettingProvider = vatSettingProvider;
  }

  public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    var defaultCustomer = await _customerRepo.GetBySpecAsync(new GetDefaultCashCustomerSpec(), cancellationToken);
    if (defaultCustomer is null)
    {
      throw new NotFoundException(nameof(defaultCustomer));
    }

    var cashPaymentMethod =
      await _paymentMethodRepo.GetBySpecAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken);
    if (cashPaymentMethod is null)
    {
      throw new NotFoundException(nameof(cashPaymentMethod));
    }

    // var stopWatch = new Stopwatch();
    // stopWatch.Start();
    // for (int i = 0; i < 1000; i++)
    // {
    //   var x = await _sequenceGenerator.NextFormatted(nameof(Order));
    // }
    //
    // stopWatch.Stop();
    // Console.WriteLine("=>>> " + stopWatch.Elapsed);

    var orderNumber = await _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(defaultCustomer.Id, orderNumber, DateTime.Now);
    order.OrderItems = new HashSet<OrderItem>();

    foreach (var item in request.Items)
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
    order = order.SetInvoiceQrCode(invoiceQrCode);
    await _repository.AddAsync(order, cancellationToken);

    var cashPayment = new OrderPayment(order.Id, cashPaymentMethod.Id, order.NetAmount);
    await _paymentRepo.AddAsync(cashPayment, cancellationToken);

    var newOrder =
      await _repository.GetBySpecAsync((ISpecification<Order, OrderDto>)new GetOrderDetailByIdSpec(order.Id),
        cancellationToken);
    if (newOrder == null)
    {
      throw new NotFoundException($"Order {order.OrderNumber} failed to successfully saved");
    }

    return newOrder;
  }
}