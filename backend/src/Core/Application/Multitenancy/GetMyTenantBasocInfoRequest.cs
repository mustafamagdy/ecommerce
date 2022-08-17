using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;

namespace FSH.WebApi.Application.Multitenancy;

public class GetMyTenantBasicInfoRequest : IRequest<BasicTenantInfoDto>
{
}

public class GetMyTenantBasicInfoRequestHandler : IRequestHandler<GetMyTenantBasicInfoRequest, BasicTenantInfoDto>
{
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IReadRepository<Branch> _branchRepo;
  private readonly IStringLocalizer _t;
  private readonly FSHTenantInfo _currentTenant;

  public GetMyTenantBasicInfoRequestHandler(IReadNonAggregateRepository<FSHTenantInfo> repo, IReadRepository<Branch> branchRepo,
    IStringLocalizer<GetMyTenantBasicInfoRequestHandler> localizer, FSHTenantInfo currentTenant)
  {
    _repo = repo;
    _branchRepo = branchRepo;
    _t = localizer;
    _currentTenant = currentTenant;
  }

  public async Task<BasicTenantInfoDto> Handle(GetMyTenantBasicInfoRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.FirstOrDefaultAsync(new GetTenantBasicInfoSpec(_currentTenant.Id))
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), _currentTenant.Id]);

    var tenantBranchSpec = new TenantBranchSpec(_currentTenant.Id);
    var branches = await _branchRepo.ListAsync(tenantBranchSpec);

    var tenantDto = tenant.Adapt<BasicTenantInfoDto>();
    tenantDto.Branches = branches.Adapt<List<BranchDto>>();

    return tenantDto;
  }
}