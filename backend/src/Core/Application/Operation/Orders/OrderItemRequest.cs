using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderItemRequest : IRequest<OrderItemDto>
{
  public Guid ItemId { get; set; }
  public int Qty { get; set; }
}

public class OrderItemRequestValidator : CustomValidator<OrderItemRequest>
{
  public OrderItemRequestValidator(IStringLocalizer<IBaseRequest> t, IReadRepository<ServiceCatalog> serviceCatalogRepo)
  {
    RuleFor(p => p.Qty)
      .GreaterThan(0)
      .LessThan(100);

    RuleFor(p => p.ItemId)
      .NotEmpty()
      .MustAsync(async (request, guid, cancellationToken)
        => await serviceCatalogRepo.AnyAsync(new GetServiceCatalogDetailByIdSpec(guid), cancellationToken))
      .WithMessage(t["Service catalog item not found"]);
  }
}