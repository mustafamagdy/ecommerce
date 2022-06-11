using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.MultiTenancy;

public class Subscription : BaseEntity
{
  public bool DefaultMonthly { get; set; }
  public string Name { get; set; }
  public int Days { get; set; }
  public decimal MonthlyPrice { get; set; }
  public decimal? YearlyPrice { get; set; }
}

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

public class SubscriptionPayment : Payment
{
  public SubscriptionPayment(decimal amount, Guid paymentMethodId)
    : base(amount, paymentMethodId)
  {
  }
}