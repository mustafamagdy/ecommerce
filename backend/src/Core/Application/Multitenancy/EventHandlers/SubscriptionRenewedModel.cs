namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class SubscriptionRenewedModel
{
  public string TenantName { get; set; }
  public decimal Amount { get; set; }
  public DateTime SubscriptionExpiryDate { get; set; }
}