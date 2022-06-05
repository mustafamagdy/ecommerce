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
  public Guid PaymentMethodId { get; set; } = default!;
  public decimal PaidAmount { get; set; } = default!;
}

public class
  CreateOrderWithNewCustomerRequestValidator : CreateOrderRequestBaseValidator<CreateOrderWithNewCustomerRequest>
{
  public CreateOrderWithNewCustomerRequestValidator(IReadRepository<Order> repository,
    IReadRepository<Customer> customerRepo, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
    RuleFor(a => a.Customer).SetValidator(new CreateSimpleCustomerRequestValidator(customerRepo, t));

    RuleFor(p => p.PaymentMethodId)
      .NotEmpty();

    // todo: validate amount for cash
    RuleFor(p => p.PaidAmount)
      .GreaterThanOrEqualTo(0)
      .LessThanOrEqualTo(1000);
  }
}

public class CreateOrderWithNewCustomerRequestHandler : IRequestHandler<CreateOrderWithNewCustomerRequest, OrderDto>
{
  private readonly IRepositoryWithEvents<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly ICreateOrderHelper _orderHelper;

  public CreateOrderWithNewCustomerRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<PaymentMethod> paymentMethodRepo, IRepositoryWithEvents<Customer> customerRepo)
  {
    _orderHelper = orderHelper;
    _paymentMethodRepo = paymentMethodRepo;
    _customerRepo = customerRepo;
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

    var order = await _orderHelper.CreateOrder(request.Items, customer, paymentMethod, cancellationToken);

    return order.Adapt<OrderDto>();
  }
}