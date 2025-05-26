using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Operation.Orders;

public class CreateCashOrderRequest : BaseOrderRequest, IRequest<OrderDto>
{
    public List<OrderPaymentAmount> Payments { get; set; } = new();
}

public class CreateCashOrderRequestValidator : CreateOrderRequestBaseValidator<CreateCashOrderRequest>
{
    public CreateCashOrderRequestValidator(IReadRepository<ServiceCatalog> serviceCatalogRepo,
        IStringLocalizer<IBaseRequest> t)
        : base(t, serviceCatalogRepo)
    {
        When(a => a.Payments.Count > 0, () =>
        {
            RuleFor(a => a.Payments)
                .ForEach(a => a.SetValidator(new OrderPaymentAmountValidator()));
        });
    }
}

public class CreateCashOrderRequestHandler : IRequestHandler<CreateCashOrderRequest, OrderDto>
{
    private readonly IReadRepository<Customer> _customerRepo;
    private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
    private readonly IStringLocalizer _t;
    private readonly ICreateOrderHelper _orderHelper;

    public CreateCashOrderRequestHandler(ICreateOrderHelper orderHelper, IReadRepository<Customer> customerRepo,
        IReadRepository<PaymentMethod> paymentMethodRepo, IStringLocalizer<CreateCashOrderRequestHandler> localizer)
    {
        _orderHelper = orderHelper;
        _customerRepo = customerRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _t = localizer;
    }

    public async Task<OrderDto> Handle(CreateCashOrderRequest request, CancellationToken cancellationToken)
    {
        var defaultCustomer =
            await _customerRepo.FirstOrDefaultAsync(new GetDefaultCashCustomerSpec(), cancellationToken);
        if (defaultCustomer is null)
        {
            throw new NotFoundException(_t["Cash customer not found"]);
        }

        var cashPaymentMethod =
            await _paymentMethodRepo.FirstOrDefaultAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken);
        if (cashPaymentMethod is null)
        {
            throw new NotFoundException(_t["Cash payment method not configured"]);
        }

        var order = await _orderHelper.CreateCashOrder(request.Items, defaultCustomer, new List<OrderPaymentAmount>()
            {
                new()
                {
                    Amount = 0,
                    PaymentMethodId = cashPaymentMethod.Id
                }
            },
            cancellationToken);

        return order.Adapt<OrderDto>();
    }
}