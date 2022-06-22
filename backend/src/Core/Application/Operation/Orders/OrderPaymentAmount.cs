namespace FSH.WebApi.Application.Operation.Orders;

public class OrderPaymentAmount
{
  public Guid PaymentMethodId { get; set; } = default!;
  public decimal Amount { get; set; } = default!;
}

public class OrderPaymentAmountValidator : CustomValidator<OrderPaymentAmount>
{
  public OrderPaymentAmountValidator()
  {
    RuleFor(p => p.PaymentMethodId)
      .NotEmpty();

    // todo: validate amount for cash
    RuleFor(p => p.Amount)
      .GreaterThanOrEqualTo(0)
      .LessThanOrEqualTo(1000);
  }
}
