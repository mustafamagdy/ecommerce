namespace FSH.WebApi.Domain.MultiTenancy;

public class SubscriptionHistory : BaseEntity
{
  public SubscriptionHistory()
  {
  }

  public SubscriptionHistory(string tenantId, Guid subscriptionId, DateTime startDate, int days, decimal price)
  {
    TenantId = tenantId;
    SubscriptionId = subscriptionId;
    StartDate = startDate;
    Price = price;
    ExpiryDate = startDate.AddDays(days);
  }

  public string TenantId { get; set; }
  public FSHTenantInfo Tenant { get; set; }
  public decimal Price { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime ExpiryDate { get; set; }

  public Guid SubscriptionId { get; private set; }
  public virtual Subscription Subscription { get; set; } = default!;
}