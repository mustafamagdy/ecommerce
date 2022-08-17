namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class TenantCreatedEmailModel
{
  public string TenantName { get; set; }
  public string AdminEmail { get; set; }
  public string SiteUrl { get; set; }
  public DateTime SubscriptionExpiryDate { get; set; }
}