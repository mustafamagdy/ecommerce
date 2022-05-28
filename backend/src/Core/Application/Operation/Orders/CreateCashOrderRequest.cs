using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.Operation;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest : IRequest<OrderItemDto>
{
  public Guid ItemId { get; set; }
}

public class CreateCashOrderRequest : IRequest<OrderDto>
{
  public List<OrderItemRequest> Items { get; set; }
}

public class CreateCashOrderRequestValidator : CustomValidator<CreateCashOrderRequest>
{
  public CreateCashOrderRequestValidator(IReadRepository<Order> repository,
    IStringLocalizer<CreateCashOrderRequestValidator> T) =>
    RuleFor(p => p.Items)
      .NotEmpty();
}

public class GetDefaultCashCustomerSpec : Specification<Customer>, ISingleResultSpecification
{
  public GetDefaultCashCustomerSpec() => Query.Where(a => a.CashDefault);
}

public class GetServiceCatalogDetailByIdSpec : Specification<ServiceCatalog, ServiceCatalogDto>, ISingleResultSpecification
{
  public GetServiceCatalogDetailByIdSpec(Guid serviceCatalogId) =>
    Query
      .Include(a => a.Product)
      .Include(a => a.Service)
      .Where(a => a.Id == serviceCatalogId);
}

public class GetOrderDetailByIdSpec : Specification<Order, OrderDto>, ISingleResultSpecification
{
  public GetOrderDetailByIdSpec(Guid serviceCatalogId) =>
    Query
      .Include(a => a.Customer)
      .Include(a => a.OrderItems)
      .ThenInclude(a => a.ServiceCatalog)
      .ThenInclude(a => a.Product)
      .Include(a => a.OrderItems)
      .ThenInclude(a => a.ServiceCatalog)
      .ThenInclude(a => a.Product)
      .Where(a => a.Id == serviceCatalogId);
}

public class CreateCashOrderRequestHandler : IRequestHandler<CreateCashOrderRequest, OrderDto>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<Order> _repository;
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IRepository<OrderItem> _orderItemRepo;

  public CreateCashOrderRequestHandler(IRepositoryWithEvents<Order> repository, IReadRepository<Customer> customerRepo,
    IRepository<OrderItem> orderItemRepo, IReadRepository<ServiceCatalog> serviceCatalogRepo)
  {
    _repository = repository;
    _customerRepo = customerRepo;
    _orderItemRepo = orderItemRepo;
    _serviceCatalogRepo = serviceCatalogRepo;
  }git p

  public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    var defaultCustomer = await _customerRepo.GetBySpecAsync(new GetDefaultCashCustomerSpec(), cancellationToken);
    if (defaultCustomer is null)
    {
      throw new NotFoundException(nameof(defaultCustomer));
    }

    //todo: order number generator
    var orderNumber = "1234";
    var order = new Order(defaultCustomer.Id, orderNumber);
    await _repository.AddAsync(order, cancellationToken);

    //todo: get vat percentage from system settings
    decimal vatPercentage = 0.15M;
    var items = new List<OrderItem>();
    foreach (OrderItemRequest item in request.Items)
    {
      var serviceItem = await _serviceCatalogRepo.GetBySpecAsync((ISpecification<ServiceCatalog, ServiceCatalogDto>)new GetServiceCatalogDetailByIdSpec(item.ItemId), cancellationToken);
      if (serviceItem is null)
      {
        throw new ArgumentNullException(nameof(serviceItem));
      }

      items.Add(new OrderItem(serviceItem.ServiceName, serviceItem.ProductName, item.ItemId, serviceItem.Price, vatPercentage, order.Id));
    }

    await _orderItemRepo.AddRangeAsync(items, cancellationToken);

    var newOrder = await _repository.GetBySpecAsync(new GetOrderDetailByIdSpec(order.Id), cancellationToken);

    return newOrder.Adapt<OrderDto>();
  }
}