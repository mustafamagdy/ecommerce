namespace FSH.WebApi.Application.Multitenancy;

public class GetBasicTenantInfoRequest : IRequest<BasicTenantInfoDto>
{
  public string TenantId { get; set; } = default!;

  public GetBasicTenantInfoRequest(string tenantId) => TenantId = tenantId;
}

public class GetBasicTenantInfoRequestValidator : CustomValidator<GetBasicTenantInfoRequest>
{
  public GetBasicTenantInfoRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class GetBasicTenantInfoRequestHandler : IRequestHandler<GetBasicTenantInfoRequest, BasicTenantInfoDto>
{
  private readonly ITenantService _tenantService;

  public GetBasicTenantInfoRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

  public Task<BasicTenantInfoDto> Handle(GetBasicTenantInfoRequest request, CancellationToken cancellationToken)
  {
    return _tenantService.GetBasicInfoByIdAsync(request.TenantId);
  }
}