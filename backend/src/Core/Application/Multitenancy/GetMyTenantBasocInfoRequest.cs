using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class GetMyTenantBasicInfoRequest : IRequest<BasicTenantInfoDto>
{
}

public class GetMyTenantBasicInfoRequestHandler : IRequestHandler<GetMyTenantBasicInfoRequest, BasicTenantInfoDto>
{
  private readonly ITenantService _tenantService;
  private readonly FSHTenantInfo _currentTenant;

  public GetMyTenantBasicInfoRequestHandler(ITenantService tenantService, FSHTenantInfo currentTenant)
  {
    _tenantService = tenantService;
    _currentTenant = currentTenant;
  }

  public Task<BasicTenantInfoDto> Handle(GetMyTenantBasicInfoRequest request, CancellationToken cancellationToken)
    => _tenantService.GetBasicInfoByIdAsync(_currentTenant.Id);
}