using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateOrderWithNewCustomerRequest : BaseOrderRequest, IRequest<OrderDto>
{
  public CreateSimpleCustomerRequest Customer { get; set; } = default!;
  public List<OrderPaymentAmount> Payments { get; set; }
}

public class
  CreateOrderWithNewCustomerRequestValidator : CreateOrderRequestBaseValidator<CreateOrderWithNewCustomerRequest>
{
  public CreateOrderWithNewCustomerRequestValidator(IReadRepository<Order> repository,
    IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(a => a.Customer).SetValidator(new CreateSimpleCustomerRequestValidator(customerRepo, t));

    RuleFor(a => a.Payments)
      .NotEmpty()
      .ForEach(a => a.SetValidator(new OrderPaymentAmountValidator()));
  }
}

public class CreateOrderWithNewCustomerRequestHandler : IRequestHandler<CreateOrderWithNewCustomerRequest, OrderDto>
{
  private readonly IRepositoryWithEvents<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly ICreateOrderHelper _orderHelper;
  private readonly IStringLocalizer _t;

  public CreateOrderWithNewCustomerRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<PaymentMethod> paymentMethodRepo, IRepositoryWithEvents<Customer> customerRepo, IStringLocalizer<CreateOrderWithNewCustomerRequestHandler> localizer)
  {
    _orderHelper = orderHelper;
    _paymentMethodRepo = paymentMethodRepo;
    _customerRepo = customerRepo;
    _t = localizer;
  }

  public async Task<OrderDto> Handle(CreateOrderWithNewCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = new Customer(request.Customer.Name, request.Customer.PhoneNumber);
    customer = await _customerRepo.AddAsync(customer, cancellationToken);
    if (customer is null)
    {
      throw new InternalServerException(_t["Failed to save new customer data"]);
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