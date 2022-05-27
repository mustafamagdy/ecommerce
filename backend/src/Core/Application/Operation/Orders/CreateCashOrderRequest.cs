using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.Operation;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest
{
  public Guid ItemId { get; set; }
}

public class CreateCashOrderRequest
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
  }

  public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    var defaultCustomer = await _customerRepo.GetBySpecAsync(new GetDefaultCashCustomerSpec(), cancellationToken);
    if (defaultCustomer is null)
    {
      throw new NotFoundException(nameof(defaultCustomer));
    }

    var order = new Order(defaultCustomer.Id);
    await _repository.AddAsync(order, cancellationToken);

    var items = new List<OrderItem>();
    foreach (OrderItemRequest item in request.Items)
    {
      var serviceItem =  (ISingleResultSpecification<ServiceCatalog, ServiceCatalogDto>)(await _serviceCatalogRepo.GetBySpecAsync(new GetServiceCatalogDetailByIdSpec(item.ItemId), cancellationToken));
      items.Add(new OrderItem(serviceItem, item.ItemId));
    }

    await _orderItemRepo.AddAsync(items);

    return order.Adapt<OrderDto>();
  }
}