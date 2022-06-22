namespace FSH.WebApi.Application.Multitenancy;

public class TenantDto : IDto
{
  public TenantDto()
  {
    Subscriptions = new List<TenantSubscriptionDto>();
    Branches = new List<BranchDto>();
  }

  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string? DatabaseName { get; set; }
  public string AdminEmail { get; set; } = default!;
  public bool IsActive { get; set; }
  public string? Issuer { get; set; }

  public TenantSubscriptionDto? ActiveSubscription { get; set; }
  public List<TenantSubscriptionDto> Subscriptions { get; set; }
  public List<BranchDto> Branches { get; set; } = default!;
}