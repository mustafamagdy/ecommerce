namespace FSH.WebApi.Application.Multitenancy;

public class TenantSubscriptionDto
{
  public string Id { get; set; } = default!;
  public string TenantId { get; set; } = default!;
  public DateTime ExpiryDate { get; set; }
  public bool IsDemo { get; set; }
}