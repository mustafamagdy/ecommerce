using System.Diagnostics;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Application.Settings.Vat;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Finance;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderPaymentDto : IDto
{
  public Guid OrderId { get; set; }
  public Guid PaymentId { get; set; }
  public Guid PaymentMethodId { get; set; }
  public decimal Amount { get; set; }
  public string PaymentMethodName { get; set; }
}

public class PayForOrderRequest : IRequest<OrderPaymentDto>
{
  public Guid OrderId { get; set; }
  public decimal Amount { get; set; }
  public Guid? PaymentMethodId { get; set; }
}

public class PayForOrderRequestValidator : CustomValidator<PayForOrderRequest>
{
  public PayForOrderRequestValidator()
  {
    RuleFor(a => a.Amount).GreaterThan(0);
    RuleFor(a => a.OrderId).NotEmpty();
  }
}

public class PayForOrderRequestHandler : IRequestHandler<PayForOrderRequest, OrderPaymentDto>
{
  private readonly IRepositoryWithEvents<Order> _repo;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IStringLocalizer _t;
  private readonly ICashRegisterResolver _cashRegisterResolver;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IRepositoryWithEvents<CashRegister> _cashRegisterRepo;
  private readonly ISystemTime _systemTime;

  public PayForOrderRequestHandler(IRepositoryWithEvents<Order> repo, IReadRepository<PaymentMethod> paymentMethodRepo,
    IStringLocalizer<PayForOrderRequestHandler> locaizer, ICashRegisterResolver cashRegisterResolver, IHttpContextAccessor httpContextAccessor,
    IApplicationUnitOfWork uow, IRepositoryWithEvents<CashRegister> cashRegisterRepo, ISystemTime systemTime)
  {
    _repo = repo;
    _paymentMethodRepo = paymentMethodRepo;
    _t = locaizer;
    _cashRegisterResolver = cashRegisterResolver;
    _httpContextAccessor = httpContextAccessor;
    _uow = uow;
    _cashRegisterRepo = cashRegisterRepo;
    _systemTime = systemTime;
  }

  public async Task<OrderPaymentDto> Handle(PayForOrderRequest request, CancellationToken cancellationToken)
  {
    var order = await _repo.FirstOrDefaultAsync(new SingleResultSpecification<Order>()
      .Query.Include(a => a.Customer)
      .Where(a => a.Id == request.OrderId)
      .Specification, cancellationToken);
    if (order is null)
    {
      throw new NotFoundException(_t["Order {0} not found", request.OrderId]);
    }

    var pm = request.PaymentMethodId == null
      ? await _paymentMethodRepo.FirstOrDefaultAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken)
      : await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId, cancellationToken);
    if (pm is null)
    {
      throw new NotFoundException(_t["Payment method not found"]);
    }

    var cashRegisterId = await _cashRegisterResolver.Resolve(_httpContextAccessor.HttpContext);
    var register = await _cashRegisterRepo.GetByIdAsync(cashRegisterId, cancellationToken)
                   ?? throw new InvalidOperationException("Cash register is not configured correctly for the request");

    register.AddOperation(new ActivePaymentOperation(register, pm.Id, request.Amount, _systemTime.Now, PaymentOperationType.In));
    var payment = new OrderPayment(order.Id, pm.Id, request.Amount);

    order.AddPayment(payment);

    await _uow.CommitAsync(cancellationToken);

    return payment.Adapt<OrderPaymentDto>();
  }
}