namespace FSH.WebApi.Application.Multitenancy;

public class TenantDto : IDto
{
  public TenantDto()
  {
    Branches = new List<BranchDto>();
  }

  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string? DatabaseName { get; set; }
  public string AdminEmail { get; set; } = default!;
  public bool Active { get; set; }
  public string? Issuer { get; set; }

  public ProdTenantSubscriptionDto? ProdSubscription { get; set; }
  public Guid? ProdSubscriptionId { get; set; }
  public DemoTenantSubscriptionDto? DemoSubscription { get; set; }
  public Guid? DemoSubscriptionId { get; set; }
  public TrainTenantSubscriptionDto? TrainSubscription { get; set; }
  public Guid? TrainSubscriptionId { get; set; }
  public List<BranchDto> Branches { get; set; } = default!;
}