using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateOrderWithNewCustomerRequest : BaseOrderRequest, IRequest<OrderDto>
{
  public CreateSimpleCustomerRequest Customer { get; set; }
  public Guid PaymentMethodId { get; set; }
  public decimal PaidAmount { get; set; }
}

public class CreateOrderWithNewCustomerRequestValidator : CreateOrderRequestBaseValidator<CreateOrderWithNewCustomerRequest>
{
  public CreateOrderWithNewCustomerRequestValidator(IReadRepository<Order> repository, IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(a => a.Customer).SetValidator(new CreateSimpleCustomerRequestValidator(customerRepo, t));

    RuleFor(p => p.PaymentMethodId)
      .NotEmpty();

    //todo: validate amount for cash

    RuleFor(p => p.PaidAmount)
      .GreaterThanOrEqualTo(0)
      .LessThanOrEqualTo(1000);
  }
}

public class CreateOrderWithNewCustomerRequestHandler : IRequestHandler<CreateOrderWithNewCustomerRequest, OrderDto>
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IRepositoryWithEvents<Customer> _customerRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IRepositoryWithEvents<OrderPayment> _paymentRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;
  private readonly IInvoiceBarcodeGenerator _barcodeGenerator;
  private readonly IVatSettingProvider _vatSettingProvider;

  public CreateOrderWithNewCustomerRequestHandler(IRepositoryWithEvents<Order> repository, IRepositoryWithEvents<Customer> customerRepo, IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator, IReadRepository<PaymentMethod> paymentMethodRepo, IRepositoryWithEvents<OrderPayment> paymentRepo, IInvoiceBarcodeGenerator barcodeGenerator, IVatSettingProvider vatSettingProvider)
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

  public async Task<OrderDto> Handle(CreateOrderWithNewCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = new Customer(request.Customer.Name, request.Customer.PhoneNumber);
    customer = await _customerRepo.AddAsync(customer, cancellationToken);
    if (customer is null)
    {
      throw new NotFoundException(nameof(customer));
    }

    var paymentMethod = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId, cancellationToken);
    if (paymentMethod is null)
    {
      throw new NotFoundException(nameof(paymentMethod));
    }

    string orderNumber = await _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(customer.Id, orderNumber, DateTime.Now);
    await _repository.AddAsync(order, cancellationToken);

    var items = new List<OrderItem>();
    foreach (var item in request.Items)
    {
      var serviceItem = await _serviceCatalogRepo.GetBySpecAsync((ISpecification<ServiceCatalog, ServiceCatalogDto>)new GetServiceCatalogDetailByIdSpec(item.ItemId), cancellationToken);
      if (serviceItem is null)
      {
        throw new ArgumentNullException(nameof(serviceItem));
      }

      items.Add(new OrderItem(serviceItem.ServiceName, serviceItem.ProductName, item.ItemId, item.Qty, serviceItem.Price, TEMPHelper.VatPercent(), order.Id));
    }

    await _orderItemRepo.AddRangeAsync(items, cancellationToken);
    var barcodeInfo = new KsaInvoiceBarcodeInfoInfo(_vatSettingProvider.LegalEntityName,
      _vatSettingProvider.VatRegNo, order.OrderDate, order.TotalAmount, order.TotalVat);

    string invoiceQrCode = _barcodeGenerator.ToBase64(barcodeInfo);
    order = order.SetInvoiceQrCode(invoiceQrCode);
    await _repository.UpdateAsync(order, cancellationToken);

    var cashPayment = new OrderPayment(order.Id, paymentMethod.Id, request.PaidAmount);
    await _paymentRepo.AddAsync(cashPayment, cancellationToken);

    var newOrder = await _repository.GetBySpecAsync((ISpecification<Order, OrderDto>)new GetOrderDetailByIdSpec(order.Id), cancellationToken);
    if (newOrder == null)
    {
      throw new NotFoundException($"Order {order.OrderNumber} failed to successfully saved");
    }

    return newOrder;
  }
}