using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateOrderWithNewCustomerRequest : BaseOrderRequest, IRequest<OrderDto>
{
  public CreateSimpleCustomerRequest Customer { get; set; }
}

public class CreateOrderWithNewCustomerRequestValidator : CreateOrderRequestBaseValidator<CreateOrderWithNewCustomerRequest>
{
  public CreateOrderWithNewCustomerRequestValidator(IReadRepository<Order> repository, IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(a => a.Customer).SetValidator(new CreateSimpleCustomerRequestValidator(customerRepo, t));
  }
}

public class CreateOrderWithNewCustomerRequestHandler : IRequestHandler<CreateOrderWithNewCustomerRequest, OrderDto>
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IRepositoryWithEvents<Customer> _customerRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;

  public CreateOrderWithNewCustomerRequestHandler(IRepositoryWithEvents<Order> repository, IRepositoryWithEvents<Customer> customerRepo, IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator)
  {
    _repository = repository;
    _customerRepo = customerRepo;
    _orderItemRepo = orderItemRepo;
    _serviceCatalogRepo = serviceCatalogRepo;
    _sequenceGenerator = sequenceGenerator;
  }

  public async Task<OrderDto> Handle(CreateOrderWithNewCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = new Customer(request.Customer.Name, request.Customer.PhoneNumber);
    customer = await _customerRepo.AddAsync(customer, cancellationToken);
    if (customer is null)
    {
      throw new NotFoundException(nameof(customer));
    }

    string orderNumber = _sequenceGenerator.NextFormatted(nameof(Order));
    var order = new Order(customer.Id, orderNumber);
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

    var newOrder = await _repository.GetBySpecAsync(new GetOrderDetailByIdSpec(order.Id), cancellationToken);

    return newOrder.Adapt<OrderDto>();
  }
}