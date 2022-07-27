namespace FSH.WebApi.Domain.MultiTenancy;

public class TenantDemoSubscription : BaseEntity
{
  public TenantDemoSubscription()
  {
    // SubscriptionHistory = new HashSet<SubscriptionHistory>();
  }

  public DateTime ExpiryDate { get; private set; }
  public virtual DemoSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public virtual FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  // public HashSet<SubscriptionHistory> SubscriptionHistory { get; set; }

  public TenantDemoSubscription Renew(DateTime today)
  {
    // SubscriptionHistory.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public class TenantTrainSubscription : BaseEntity
{
  public TenantTrainSubscription()
  {
    // SubscriptionHistory = new HashSet<SubscriptionHistory>();
  }

  public DateTime ExpiryDate { get; private set; }
  public virtual TrainSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public virtual FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  // public HashSet<SubscriptionHistory> SubscriptionHistory { get; set; }

  public TenantTrainSubscription Renew(DateTime today)
  {
    // SubscriptionHistory.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public class TenantProdSubscription : BaseEntity
{
  public TenantProdSubscription()
  {
  }

  public TenantProdSubscription(StandardSubscription subscription, string tenantId)
  {
    TenantId = tenantId;
    Subscription = subscription;
    SubscriptionId = subscription.Id;

    SubscriptionHistory = new HashSet<SubscriptionHistory>();
    Payments = new HashSet<SubscriptionPayment>();
  }

  public DateTime ExpiryDate { get; private set; }
  public virtual StandardSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public virtual FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  public HashSet<SubscriptionHistory> SubscriptionHistory { get; set; }
  public virtual HashSet<SubscriptionPayment> Payments { get; set; } = default!;

  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;
  public decimal Balance => SubscriptionHistory.Sum(a => a.Price) - TotalPaid;

  public TenantProdSubscription Pay(decimal amount, Guid paymentMethodId)
  {
    Payments.Add(new SubscriptionPayment(Id, amount, paymentMethodId));
    return this;
  }

  public TenantProdSubscription Renew(DateTime today)
  {
    SubscriptionHistory.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public class SubscriptionHistory : BaseEntity
{
  public SubscriptionHistory()
  {
  }

  public SubscriptionHistory(Guid tenantProdSubscriptionId, DateTime startDate, int days, decimal price)
  {
    TenantProdSubscriptionId = tenantProdSubscriptionId;
    StartDate = startDate;
    Price = price;
    ExpiryDate = startDate.AddDays(days);
  }

  public decimal Price { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime ExpiryDate { get; set; }

  public virtual TenantProdSubscription TenantProdSubscription { get; set; }
  public Guid TenantProdSubscriptionId { get; set; }
}