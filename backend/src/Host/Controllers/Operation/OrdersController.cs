using FSH.WebApi.Application.Operation;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Operation;

public class OrdersController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Orders)]
  [OpenApiOperation("Search orders using available filters.", "")]
  [HasValidSubscriptionType(SubscriptionType.Standard)]
  public Task<PaginationResponse<OrderDto>> SearchAsync(SearchOrdersRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("cash")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<OrderDto> CreateAsync(CreateCashOrderRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("with-customer")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<OrderDto> CreateAsync(CreateOrderWithNewCustomerRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  public Task<OrderDto> CreateAsync(CreateOrderRequest request)
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

  [HttpGet("pdf/{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Orders)]
  [OpenApiOperation("Export Pdf invoice receipt for an order.", "")]
  public async Task<FileResult> ExportPdfInvoiceAsync(Guid id)
  {
    (string orderNumber, var generatedPdf) = await Mediator.Send(new ExportOrderInvoiceRequest { OrderId = id });
    return File(generatedPdf, "application/octet-stream", $"invoice_{orderNumber}.pdf");
  }

  [HttpPut("cancel/{id:guid}")]
  [MustHavePermission(FSHAction.Cancel, FSHResource.Orders)]
  [OpenApiOperation("Cancel a order with its all payments.", "")]
  public Task CancelAsync(Guid id)
  {
    return Mediator.Send(new CancelOrderWithPaymentsRequest(id));
  }

  // [HttpPut("{id:guid}")]
  // [MustHavePermission(FSHAction.Update, FSHResource.Orders)]
  // [OpenApiOperation("Update a order.", "")]
  // public async Task<ActionResult<Guid>> UpdateAsync(UpdateOrderRequest request, Guid id)
  // {
  //   return id != request.Id
  //     ? BadRequest()
  //     : Ok(await Mediator.Send(request));
  // }
  //
  // [HttpDelete("{id:guid}")]
  // [MustHavePermission(FSHAction.Delete, FSHResource.Orders)]
  // [OpenApiOperation("Delete a order.", "")]
  // public Task<Guid> DeleteAsync(Guid id)
  // {
  //   return Mediator.Send(new DeleteOrderRequest(id));
  // }
}