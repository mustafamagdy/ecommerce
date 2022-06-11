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
  public List<OrderPaymentAmount> Payments { get; set; }
}

public class CreateOrderRequestValidator : CreateOrderRequestBaseValidator<CreateOrderRequest>
{
  public CreateOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(p => p.CustomerId)
      .NotEmpty();

    RuleFor(a => a.Payments)
      .NotEmpty()
      .ForEach(a => a.SetValidator(new OrderPaymentAmountValidator()));
  }
}

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, OrderDto>
{
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly ICreateOrderHelper _orderHelper;
  private readonly IStringLocalizer _t;

  public CreateOrderRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<Customer> customerRepo, IReadRepository<PaymentMethod> paymentMethodRepo, IStringLocalizer<CreateOrderRequestHandler> localizer)
  {
    _orderHelper = orderHelper;
    _customerRepo = customerRepo;
    _paymentMethodRepo = paymentMethodRepo;
    _t = localizer;
  }

  public async Task<OrderDto> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
  {
    var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);
    if (customer is null)
    {
      throw new NotFoundException(_t["Customer {0} not found", request.CustomerId]);
    }

    foreach (var payment in request.Payments)
    {
      var paymentMethod = await _paymentMethodRepo.GetByIdAsync(payment.PaymentMethodId, cancellationToken);
      if (paymentMethod is null)
      {
        throw new NotFoundException(_t["Payment method {0} not found", payment.PaymentMethodId]);
      }
    }

    var order = await _orderHelper.CreateOrder(request.Items, customer, request.Payments, cancellationToken);

    return order.Adapt<OrderDto>();
  }
}