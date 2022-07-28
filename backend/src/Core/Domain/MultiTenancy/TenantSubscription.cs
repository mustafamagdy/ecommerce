namespace FSH.WebApi.Domain.MultiTenancy;

public class TenantDemoSubscription : BaseEntity
{
  private TenantDemoSubscription()
  {
    // SubscriptionHistory = new List<SubscriptionHistory>();
  }

  public DateTime ExpiryDate { get; private set; }
  public virtual DemoSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public virtual FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  // public List<SubscriptionHistory> SubscriptionHistory { get; set; }

  public TenantDemoSubscription Renew(DateTime today)
  {
    // SubscriptionHistory.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public class TenantTrainSubscription : BaseEntity
{
  private TenantTrainSubscription()
  {
    // SubscriptionHistory = new List<SubscriptionHistory>();
  }

  public DateTime ExpiryDate { get; private set; }
  public virtual TrainSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public virtual FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  // public List<SubscriptionHistory> SubscriptionHistory { get; set; }

  public TenantTrainSubscription Renew(DateTime today)
  {
    // SubscriptionHistory.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public sealed class TenantProdSubscription : BaseEntity
{
  private readonly List<SubscriptionHistory> _history = new();
  private readonly List<SubscriptionPayment> _payments = new();

  private TenantProdSubscription()
  {
  }

  public TenantProdSubscription(StandardSubscription subscription, FSHTenantInfo tenant)
  {
    Subscription = subscription;
    Tenant = tenant;
    SubscriptionId = subscription.Id;
    TenantId = tenant.Id;
  }

  public StandardSubscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  public DateTime ExpiryDate { get; private set; }

  public IReadOnlyList<SubscriptionHistory> History => _history.AsReadOnly();
  public IReadOnlyList<SubscriptionPayment> Payments => _payments.AsReadOnly();

  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;
  public decimal TotalDue => History.Sum(a => a.Price) - TotalPaid;

  public void Pay(decimal amount, Guid paymentMethodId)
  {
    _payments.Add(new SubscriptionPayment(this, amount, paymentMethodId));
  }

  public TenantProdSubscription Renew(DateTime today)
  {
    _history.Add(new SubscriptionHistory(this, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public sealed class SubscriptionHistory : BaseEntity
{
  private SubscriptionHistory()
  {
  }

  public SubscriptionHistory(TenantProdSubscription tenantProdSubscription, DateTime startDate, int days, decimal price)
  {
    TenantProdSubscription = tenantProdSubscription;
    TenantProdSubscriptionId = tenantProdSubscription.Id;
    StartDate = startDate;
    Price = price;
    ExpiryDate = startDate.AddDays(days);
  }

  public decimal Price { get; private set; }
  public DateTime StartDate { get; private set; }
  public DateTime ExpiryDate { get; private set; }

  public TenantProdSubscription TenantProdSubscription { get; set; }
  public Guid TenantProdSubscriptionId { get; set; }
}