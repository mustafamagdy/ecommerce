using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Infrastructure.Finance;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Operation;

public sealed class OrdersController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Orders)]
  [OpenApiOperation("Search orders using available filters.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<PaginationResponse<OrderDto>> SearchAsync(SearchOrdersRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost("cash")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  [RequireOpenedCashRegister]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create1))]
  public Task<OrderDto> CreateCash(CreateCashOrderRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost("with-customer")]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  [RequireOpenedCashRegister]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create))]
  public Task<OrderDto> CreateWithCustomer(CreateOrderWithNewCustomerRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Orders)]
  [OpenApiOperation("Create a new order.", "")]
  [RequireOpenedCashRegister]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create))]
  public Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Orders)]
  [OpenApiOperation("Get order details.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.GetAsync))]
  public Task<OrderDto> GetAsync(Guid id, CancellationToken cancellationToken)
  {
    return Mediator.Send(new GetOrderRequest(id), cancellationToken);
  }

  [HttpGet("pdf/{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Orders)]
  [OpenApiOperation("Export Pdf invoice receipt for an order.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Generate))]
  public async Task<FileResult> ExportPdfInvoiceAsync(Guid id, CancellationToken cancellationToken)
  {
    (string orderNumber, var generatedPdf) = await Mediator.Send(new ExportOrderInvoiceRequest { OrderId = id }, cancellationToken);
    return File(generatedPdf, "application/octet-stream", $"invoice_{orderNumber}.pdf");
  }

  [HttpPut("cancel/{id:guid}")]
  [MustHavePermission(FSHAction.Cancel, FSHResource.Orders)]
  [OpenApiOperation("Cancel a order with its all payments.", "")]
  [RequireOpenedCashRegister]
  public Task CancelAsync(Guid id, CancellationToken cancellationToken)
  {
    return Mediator.Send(new CancelOrderWithPaymentsRequest(id), cancellationToken);
  }

  [HttpPost("pay")]
  [MustHavePermission(FSHAction.Pay, FSHResource.Orders)]
  [OpenApiOperation("Export Pdf invoice receipt for an order.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Generate))]
  public Task<OrderPaymentDto> PayForOrder(PayForOrderRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost("summary-report")]
  [MustHavePermission(FSHAction.View, FSHResource.Orders)]
  [OpenApiOperation("Generate orders summary report for requested filters.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Generate))]
  public async Task<FileResult> ExportSummaryReport(OrderSummaryReportRequest request,
    [FromServices] ISystemTime time, CancellationToken cancellationToken)
  {
    var orderPdf = await Mediator.Send(request, cancellationToken);
    var reportName = $"orders_summary_{time.Now.ToShortDateString()}";
    return File(orderPdf, "application/octet-stream", $"{reportName}.pdf");
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