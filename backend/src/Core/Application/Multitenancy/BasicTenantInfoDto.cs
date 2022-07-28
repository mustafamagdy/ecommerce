namespace FSH.WebApi.Application.Multitenancy;

public class BasicTenantInfoDto
{
  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;

  public BasicSubscriptionInfoDto? ProdSubscription { get; set; }
  public BasicSubscriptionInfoDto? DemoSubscription { get; set; }
  public BasicSubscriptionInfoDto? TrainSubscription { get; set; }
  public List<BranchDto> Branches { get; set; } = default!;
  public bool Active { get; set; }
  public decimal TotalDue { get; set; }
  public decimal TotalPaid { get; set; }
}

public class BasicSubscriptionInfoDto
{
  public Guid Id { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
}