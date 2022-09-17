namespace FSH.WebApi.Application.Multitenancy;

public class ViewTenantInfoDto : IDto
{
  public string Id { get; set; }
  public string Name { get; set; }
  public string AdminEmail { get; set; }
  public string PhoneNumber { get; set; }
  public string VatNo { get; set; }
  public string Email { get; set; }
  public string Address { get; set; }
  public string AdminName { get; set; }
  public string AdminPhoneNumber { get; set; }
  public string TechSupportUserId { get; set; }
}
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