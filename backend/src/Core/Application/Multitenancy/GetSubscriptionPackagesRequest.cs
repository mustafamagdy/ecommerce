using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class SubscriptionPackageDto : IDto
{
  public Guid Id { get; set; }
  public bool Default { get; set; }
  public int ValidForDays { get; set; }
  public decimal Price { get; set; }

  public List<SubscriptionFeature> Features { get; set; }
}

public class GetSubscriptionPackagesRequest : IRequest<List<SubscriptionPackageDto>>
{
}

public class GetSubscriptionPackagesSpec : Specification<SubscriptionPackage, SubscriptionPackageDto>
{
  public GetSubscriptionPackagesSpec()
    => Query.Include(a => a.Features);
}

public class GetSubscriptionPackagesRequestHandler : IRequestHandler<GetSubscriptionPackagesRequest, List<SubscriptionPackageDto>>
{
  private readonly IReadNonAggregateRepository<SubscriptionPackage> _repo;

  public GetSubscriptionPackagesRequestHandler(IReadNonAggregateRepository<SubscriptionPackage> repo)
  {
    _repo = repo;
  }

  public Task<List<SubscriptionPackageDto>> Handle(GetSubscriptionPackagesRequest request, CancellationToken cancellationToken)
    => _repo.ListAsync(new GetSubscriptionPackagesSpec(), cancellationToken);
}