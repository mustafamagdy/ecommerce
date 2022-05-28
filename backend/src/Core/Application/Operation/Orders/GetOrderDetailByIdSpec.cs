using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class GetOrderDetailByIdSpec : Specification<Order, OrderDto>, ISingleResultSpecification
{
  public GetOrderDetailByIdSpec(Guid serviceCatalogId) =>
    Query
      .Include(a => a.Customer)
      .Include(a => a.OrderItems)
        .ThenInclude(a => a.ServiceCatalog)
          .ThenInclude(a => a.Product)
      .Include(a => a.OrderItems)
        .ThenInclude(a => a.ServiceCatalog)
          .ThenInclude(a => a.Product)
      .Where(a => a.Id == serviceCatalogId);
}