using FSH.WebApi.Application.Operation.Payments;

namespace FSH.WebApi.Host.Controllers.Operation;

public sealed class PaymentMethodsController : VersionedApiController
{
    [HttpPost("search")]
    // [MustHavePermission(FSHAction.View, FSHResource.Orders)]
    [OpenApiOperation("Generate orders summary report for requested filters.", "")]
    public async Task<PaginationResponse<PaymentMethodDto>> GetPaymentMethods(
        SearchPaymentMethodsRequest request,
        CancellationToken cancellationToken)
    {
        return await Mediator.Send(request, cancellationToken);
    }
}