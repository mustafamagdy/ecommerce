using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest : IRequest<OrderItemDto>
{
  public Guid ItemId { get; set; }
  public int Qty { get; set; }
}

public class OrderItemRequestValidator : CustomValidator<OrderItemRequest>
{
  public OrderItemRequestValidator(IStringLocalizer t)
  {
    RuleFor(p => p.Qty)
      .GreaterThan(0)
      .LessThan(100);

    RuleFor(p => p.ItemId)
      .NotEmpty();
  }
}