using System.Diagnostics;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateCashOrderRequest : BaseOrderRequest, IRequest<OrderDto>
{
}

public class CreateCashOrderRequestValidator : CreateOrderRequestBaseValidator<CreateCashOrderRequest>
{
  public CreateCashOrderRequestValidator(IReadRepository<Order> repository, IStringLocalizer<IBaseRequest> t)
    : base(t)
  {
  }
}

public class CreateCashOrderRequestHandler : IRequestHandler<CreateCashOrderRequest, OrderDto>
{
  // Add Domain Events automatically by using IRepositoryWithEvents
  private readonly IReadRepository<Customer> _customerRepo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;

  private readonly ICreateOrderHelper _orderHelper;

  public CreateCashOrderRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<Customer> customerRepo, IReadRepository<PaymentMethod> paymentMethodRepo)
  {
    _orderHelper = orderHelper;
    _customerRepo = customerRepo;
    _paymentMethodRepo = paymentMethodRepo;
  }

  public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    var defaultCustomer = await _customerRepo.GetBySpecAsync(new GetDefaultCashCustomerSpec(), cancellationToken);
    if (defaultCustomer is null)
    {
      throw new NotFoundException(nameof(defaultCustomer));
    }

    var cashPaymentMethod =
      await _paymentMethodRepo.GetBySpecAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken);
    if (cashPaymentMethod is null)
    {
      throw new NotFoundException(nameof(cashPaymentMethod));
    }

    var order = await _orderHelper.CreateOrder(request.Items, defaultCustomer, cashPaymentMethod, cancellationToken);

    return order.Adapt<OrderDto>();
  }
}