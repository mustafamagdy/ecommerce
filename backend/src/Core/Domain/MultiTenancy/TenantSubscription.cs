namespace FSH.WebApi.Domain.MultiTenancy;

public class TenantDemoSubscription : TenantSubscription<DemoSubscription>
{
  private TenantDemoSubscription()
  {
  }

  public TenantDemoSubscription(DemoSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }
}

public class TenantTrainSubscription : TenantSubscription<TrainSubscription>
{
  private TenantTrainSubscription()
  {
  }

  public TenantTrainSubscription(TrainSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }
}

public sealed class TenantProdSubscription : TenantSubscription<StandardSubscription>
{
  private readonly List<SubscriptionPayment> _payments = new();

  private TenantProdSubscription()
  {
  }

  public TenantProdSubscription(StandardSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }

  public IReadOnlyList<SubscriptionPayment> Payments => _payments.AsReadOnly();

  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;
  public decimal TotalDue => History.Sum(a => a.Price) - TotalPaid;

  public void Pay(decimal amount, Guid paymentMethodId)
  {
    _payments.Add(new SubscriptionPayment(this, amount, paymentMethodId));
  }
}

public abstract class TenantSubscription<T> : BaseEntity
  where T : Subscription, new()
{
  private readonly List<SubscriptionHistory> _history = new();

  protected TenantSubscription()
  {
  }

  protected TenantSubscription(T subscription, FSHTenantInfo tenant)
  {
    Subscription = subscription;
    Tenant = tenant;
    SubscriptionId = subscription.Id;
    TenantId = tenant.Id;
  }

  public DateTime ExpiryDate { get; private set; }

  public T Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  public IReadOnlyList<SubscriptionHistory> History => _history.AsReadOnly();

  public virtual TenantSubscription<T> Renew(DateTime today)
  {
    _history.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public sealed class SubscriptionHistory : BaseEntity
{
  private SubscriptionHistory()
  {
  }

  public SubscriptionHistory(Guid tenantSubscriptionId, DateTime startDate, int days, decimal price)
  {
    TenantSubscriptionId = tenantSubscriptionId;
    StartDate = startDate;
    Price = price;
    ExpiryDate = startDate.AddDays(days);
  }

  public decimal Price { get; private set; }
  public DateTime StartDate { get; private set; }
  public DateTime ExpiryDate { get; private set; }

  public Guid TenantSubscriptionId { get; set; }
}