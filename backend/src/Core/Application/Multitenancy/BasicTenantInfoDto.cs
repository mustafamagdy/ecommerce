namespace FSH.WebApi.Application.Multitenancy;

public class BasicTenantInfoDto
{
  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;

  public BasicSubscriptionInfoDto CurrentSubscription { get; set; } = default!;
  public List<BranchDto> Branches { get; set; } = default!;
}

public class BasicSubscriptionInfoDto
{
  public string Id { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
}