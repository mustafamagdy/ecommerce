using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class GetTenantRequest : IRequest<TenantDto>
{
  public string TenantId { get; set; } = default!;

  public GetTenantRequest(string tenantId) => TenantId = tenantId;
}

public class GetTenantRequestValidator : CustomValidator<GetTenantRequest>
{
  public GetTenantRequestValidator() =>
    RuleFor(t => t.TenantId)
      .NotEmpty();
}

public class GetTenantInfoSpec : Specification<FSHTenantInfo, TenantDto>, ISingleResultSpecification
{
  public GetTenantInfoSpec(string tenantId) =>
    Query
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.History)
      .Include(a => a.ProdSubscription)
      .ThenInclude(a => a.Payments)
      .Include(a => a.DemoSubscription)
      .Include(a => a.TrainSubscription)
      .Where(a => a.Id == tenantId);
}

public class GetTenantRequestHandler : IRequestHandler<GetTenantRequest, TenantDto>
{
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _repo;
  private readonly IStringLocalizer _t;

  public GetTenantRequestHandler(IReadNonAggregateRepository<FSHTenantInfo> repo, IStringLocalizer<GetTenantRequestHandler> localizer)
  {
    _repo = repo;
    _t = localizer;
  }

  public async Task<TenantDto> Handle(GetTenantRequest request, CancellationToken cancellationToken)
    => await _repo.FirstOrDefaultAsync(new GetTenantInfoSpec(request.TenantId), cancellationToken)
       ?? throw new NotFoundException(_t["Tenant with id {0} not found", request.TenantId]);
}