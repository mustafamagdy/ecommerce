using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateOrderRequest : BaseOrderRequest, IRequest<OrderDto>
{
  public Guid CustomerId { get; set; }
}

public class CreateOrderRequestValidator : CreateOrderRequestBaseValidator<CreateOrderRequest>
{
  public CreateOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
  }
}

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, OrderDto>
{
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;
  private readonly ITenantSequenceGenerator _sequenceGenerator;

  public CreateOrderRequestHandler(IRepositoryWithEvents<Order> repository, IReadRepository<Customer> customerRepo, IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    ITenantSequenceGenerator sequenceGenerator)
  {
    _repository = repository;
    _customerRepo = customerRepo;
    _orderItemRepo = orderItemRepo;
    _serviceCatalogRepo = serviceCatalogRepo;
    _sequenceGenerator = sequenceGenerator;
  }

  public async Task<OrderDto> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
  {
    var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);
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