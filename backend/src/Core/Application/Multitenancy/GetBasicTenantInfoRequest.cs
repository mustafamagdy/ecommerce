using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using Mapster;

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

public class GetTenantBasicInfoSpec : Specification<FSHTenantInfo, BasicTenantInfoDto>, ISingleResultSpecification
{
  public GetTenantBasicInfoSpec(string tenantId) =>
    Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History)
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.Payments)
      .Include(a => a.DemoSubscription)
      .Include(a => a.TrainSubscription)
      .Where(a => a.Id == tenantId);
}

public class GetBasicTenantInfoRequestHandler : IRequestHandler<GetBasicTenantInfoRequest, BasicTenantInfoDto>
{
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IReadRepository<Branch> _branchRepo;
  private readonly IStringLocalizer _t;

  public GetBasicTenantInfoRequestHandler(IReadNonAggregateRepository<FSHTenantInfo> repo, IReadRepository<Branch> branchRepo,
    IStringLocalizer localizer)
  {
    _repo = repo;
    _branchRepo = branchRepo;
    _t = localizer;
  }

  public async Task<BasicTenantInfoDto> Handle(GetBasicTenantInfoRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _repo.FirstOrDefaultAsync(new GetTenantBasicInfoSpec(request.TenantId))
                 ?? throw new NotFoundException(_t["{0} {1} Not Found.", nameof(FSHTenantInfo), request.TenantId]);

    var tenantBranchSpec = new TenantBranchSpec(request.TenantId);
    var branches = await _branchRepo.ListAsync(tenantBranchSpec);

    var tenantDto = tenant.Adapt<BasicTenantInfoDto>();
    tenantDto.Branches = branches.Adapt<List<BranchDto>>();

    return tenantDto;
  }
}