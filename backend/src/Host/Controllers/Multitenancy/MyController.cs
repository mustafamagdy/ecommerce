using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Multitenancy;

public class MyController : VersionedApiController
{
  [HttpGet]
  [MustHavePermission(FSHAction.ViewBasic, FSHResource.Tenants)]
  [OpenApiOperation("View my tenant basic information", "")]
  public Task<BasicTenantInfoDto> ViewTenantBasicInfo()
  {
    return Mediator.Send(new GetMyTenantBasicInfoRequest());
  }

  [HttpPost("subscription")]
  [MustHavePermission(FSHAction.ViewMy, FSHResource.Subscriptions)]
  [OpenApiOperation("View my tenant production subscription details & history.", "")]
  public Task<TenantSubscriptionDto> ViewProdSubscriptionDetails(MyTenantSubscriptionSearchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("payments")]
  [MustHavePermission(FSHAction.ViewAdvanced, FSHResource.Subscriptions)]
  [OpenApiOperation("View my tenant production subscription details & payments.", "")]
  public Task<ProdTenantSubscriptionWithPaymentDto> ViewProdSubscriptionDetailsAndPayments(MyTenantSubscriptionAndPaymentsSearchRequest request)
  {
    return Mediator.Send(request);
  }
}