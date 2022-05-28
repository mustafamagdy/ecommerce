using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class GetOrderRequest : IRequest<OrderDto>
{
  public GetOrderRequest(Guid orderId)
  {
    OrderId = orderId;
  }

  public Guid OrderId { get; private set; }
}

public class GetOrderRequestHandler : IRequestHandler<GetOrderRequest, OrderDto>
{
  private readonly IReadRepository<Order> _repository;

  public GetOrderRequestHandler(IReadRepository<Order> repository)
  {
    _repository = repository;
  }

  public async Task<OrderDto> Handle(GetOrderRequest request, CancellationToken cancellationToken)
  {
    var order = await _repository.GetBySpecAsync((ISpecification<Order, OrderDto>)new GetOrderDetailByIdSpec(request.OrderId), cancellationToken);
    if (order is null)
    {
      throw new NotFoundException(nameof(order));
    }

    return order;
  }
}