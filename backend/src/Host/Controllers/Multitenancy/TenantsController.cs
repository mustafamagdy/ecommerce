using FSH.WebApi.Application.Multitenancy;
using MediatR;

namespace FSH.WebApi.Host.Controllers.Multitenancy;

public class TenantsController : VersionNeutralApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Tenants)]
  [OpenApiOperation("Search all tenants.", "")]
  public Task<PaginationResponse<TenantDto>> GetListAsync(SearchAllTenantsRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id}")]
  [MustHavePermission(FSHAction.View, FSHResource.Tenants)]
  [OpenApiOperation("Get tenant details.", "")]
  public Task<TenantDto> GetAsync(string id)
  {
    return Mediator.Send(new GetTenantRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Tenants)]
  [OpenApiOperation("Create a new tenant.", "")]
  public Task<string> CreateAsync(CreateTenantRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("{id}/activate")]
  [MustHavePermission(FSHAction.Update, FSHResource.Tenants)]
  [OpenApiOperation("Activate a tenant.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  public Task<string> ActivateAsync(string id)
  {
    return Mediator.Send(new ActivateTenantRequest(id));
  }

  [HttpPost("{id}/deactivate")]
  [MustHavePermission(FSHAction.Update, FSHResource.Tenants)]
  [OpenApiOperation("Deactivate a tenant.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Register))]
  public Task<string> DeactivateAsync(string id)
  {
    return Mediator.Send(new DeactivateTenantRequest(id));
  }

  [HttpGet("{tenantId}/subscriptions")]
  [MustHavePermission(FSHAction.View, FSHResource.Subscriptions)]
  [OpenApiOperation("Get a list of active subscriptions for a tenant.", "")]
  public Task<List<TenantSubscriptionDto>> GetActiveSubscriptions(string tenantId)
  {
    return Mediator.Send(new GetTenantSubscriptionsRequest(tenantId, activeSubscription: null));
  }

  [HttpPost("renew")]
  [MustHavePermission(FSHAction.Update, FSHResource.Subscriptions)]
  [OpenApiOperation("Renew subscription for a tenant", "")]
  public Task<string> RenewSubscription(RenewSubscriptionRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("pay")]
  [MustHavePermission(FSHAction.Update, FSHResource.Subscriptions)]
  [OpenApiOperation("Pay for tenant subscription", "")]
  public Task<Unit> RenewSubscription(PayForSubscriptionRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id}/info")]
  [MustHavePermission(FSHAction.ViewBasic, FSHResource.Tenants)]
  [OpenApiOperation("Get tenant details.", "")]
  public Task<BasicTenantInfoDto> GetBasicAsync(string id)
  {
    return Mediator.Send(new GetBasicTenantInfoRequest(id));
  }
}