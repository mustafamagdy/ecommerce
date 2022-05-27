using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Operation;

public class OrdersController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Orders)]
  [OpenApiOperation("Search orders using available filters.", "")]
  [HasValidSubscriptionLevel(SubscriptionLevel.Basic)]
  public Task<PaginationResponse<OrderDto>> SearchAsync(SearchOrdersRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Orders)]
  [OpenApiOperation("Get order details.", "")]
  public Task<OrderDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetOrderRequest(id));
  }

  [HttpPost("cash")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<Guid> CreateAsync(CreateCashOrderRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("with-customer")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<Guid> CreateAsync(CreateOrderWithNewCustomerRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<Guid> CreateAsync(CreateOrderRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.Orders)]
  [OpenApiOperation("Update a order.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateOrderRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Orders)]
  [OpenApiOperation("Delete a order.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteOrderRequest(id));
  }

  [HttpPost("generate-random")]
  [MustHavePermission(FSHAction.Generate, FSHResource.Orders)]
  [OpenApiOperation("Generate a number of random orders.", "")]
  public Task<string> GenerateRandomAsync(GenerateRandomOrderRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete("delete-random")]
  [MustHavePermission(FSHAction.Clean, FSHResource.Orders)]
  [OpenApiOperation("Delete the orders generated with the generate-random call.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<string> DeleteRandomAsync()
  {
    return Mediator.Send(new DeleteRandomOrderRequest());
  }
}