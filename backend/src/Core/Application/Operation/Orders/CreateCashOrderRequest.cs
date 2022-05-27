using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest {}

public class CreateCashOrderRequest
{
  public List<OrderItemRequest> Type { get; set; }
}

public class CreateCashOrderRequestValidator : CustomValidator<CreateCashOrderRequest>
{
  public CreateCashOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<CreateCashOrderRequestValidator> T) =>
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new OrderByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["Order {0} already Exists.", name]);
}

public class CreateCashOrderRequestHandler : IRequestHandler<CreateCashOrderRequest, OrderDto>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IRepositoryWithEvents<Order> _repository;

  public CreateCashOrderRequestHandler(IRepositoryWithEvents<Order> repository) => _repository = repository;

  public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    var brand = new Order(request.Name, request.Description);

    await _repository.AddAsync(brand, cancellationToken);

    return brand;
  }
}