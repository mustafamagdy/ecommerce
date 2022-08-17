namespace FSH.WebApi.Application.Multitenancy.EventHandlers;

public class PayForSubscriptionEmailModel
{
  public string TenantName { get; set; }
  public decimal Amount { get; set; }
  public string PaymentMethodName { get; set; }
  public DateTime SubscriptionExpiryDate { get; set; }
}