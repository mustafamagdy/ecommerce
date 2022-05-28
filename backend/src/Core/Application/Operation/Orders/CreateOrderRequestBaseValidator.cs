namespace FSH.WebApi.Application.Operation.Orders;

public abstract class CreateOrderRequestBaseValidator<T> : CustomValidator<T>
  where T : BaseOrderRequest
{
  public CreateOrderRequestBaseValidator(IStringLocalizer<IBaseRequest> t)
  {
    RuleFor(p => p.Items)
      .NotEmpty();

    RuleForEach(a => a.Items)
      .SetValidator(new OrderItemRequestValidator(t));
  }
}