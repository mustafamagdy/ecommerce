namespace FSH.WebApi.Domain.MultiTenancy;

public class TenantSubscription : BaseEntity
{
  public TenantSubscription(string tenantId, Guid subscriptionId, DateTime startDate, decimal price, bool isDemo)
  {
    TenantId = tenantId;
    SubscriptionId = subscriptionId;
    StartDate = startDate;
    Price = price;
    IsDemo = isDemo;
  }

  public string TenantId { get; private set; }
  public bool IsDemo { get; private set; }
  public DateTime StartDate { get; private set; }
  public decimal Price { get; private set; }

  public DateTime ExpiryDate { get; private set; }

  public Guid SubscriptionId { get; private set; }
  public virtual Subscription Subscription { get; set; } = default!;

  public bool Active { get; private set; }

  public virtual HashSet<SubscriptionPayment> Payments { get; set; } = default!;
  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;

  public TenantSubscription Extend(DateTime newExpiryDate)
  {
    ExpiryDate = newExpiryDate;
    return Activate();
  }

  public TenantSubscription Activate()
  {
    Active = true;
    return this;
  }

  public TenantSubscription DeActivate()
  {
    Active = false;
    return this;
  }
}