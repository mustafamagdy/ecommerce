using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateOrderRequest : BaseOrderRequest, IRequest<OrderDto>
{
  public Guid CustomerId { get; set; }
  public Guid PaymentMethodId { get; set; }
  public decimal PaidAmount { get; set; }
}

public class CreateOrderRequestValidator : CreateOrderRequestBaseValidator<CreateOrderRequest>
{
  public CreateOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(p => p.CustomerId)
      .NotEmpty();

    RuleFor(p => p.PaymentMethodId)
      .NotEmpty();

    // todo: validate amount for cash
    RuleFor(p => p.PaidAmount)
      .GreaterThanOrEqualTo(0)
      .LessThanOrEqualTo(1000);
  }
}

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, OrderDto>
{
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly ICreateOrderHelper _orderHelper;

  public CreateOrderRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<Customer> customerRepo, IReadRepository<PaymentMethod> paymentMethodRepo)
  {
    _orderHelper = orderHelper;
    _customerRepo = customerRepo;
    _paymentMethodRepo = paymentMethodRepo;
  }

  public async Task<OrderDto> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
  {
    var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);
    if (customer is null)
    {
      throw new NotFoundException(nameof(customer));
    }

    var paymentMethod = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId, cancellationToken);
    if (paymentMethod is null)
    {
      throw new NotFoundException(nameof(paymentMethod));
    }

    var order = await _orderHelper.CreateOrder(request.Items, customer, paymentMethod, cancellationToken);

    return order.Adapt<OrderDto>();
  }
}