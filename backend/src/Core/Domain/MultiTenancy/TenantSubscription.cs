namespace FSH.WebApi.Domain.MultiTenancy;

public sealed class TenantDemoSubscription : TenantSubscription
{
  private TenantDemoSubscription()
  {
  }

  public TenantDemoSubscription(DemoSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }
}

public sealed class TenantTrainSubscription : TenantSubscription
{
  private TenantTrainSubscription()
  {
  }

  public TenantTrainSubscription(TrainSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }
}

public sealed class TenantProdSubscription : TenantSubscription
{
  private readonly List<SubscriptionPayment> _payments = new();

  private TenantProdSubscription()
  {
  }

  public TenantProdSubscription(StandardSubscription subscription, FSHTenantInfo tenant)
    : base(subscription, tenant)
  {
  }

  public IReadOnlyCollection<SubscriptionPayment> Payments => _payments.AsReadOnly();

  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;
  public decimal TotalDue => History.Sum(a => a.Price) - TotalPaid;

  public void Pay(decimal amount, Guid paymentMethodId)
  {
    var payment = new SubscriptionPayment(amount, paymentMethodId);
    payment.TenantProdSubscriptionId = Id;
    _payments.Add(payment);
  }
}

public abstract class TenantSubscription : BaseEntity
{
  private readonly List<SubscriptionHistory> _history = new();

  protected TenantSubscription()
  {
  }

  protected TenantSubscription(Subscription subscription, FSHTenantInfo tenant)
  {
    Subscription = subscription;
    Tenant = tenant;
    SubscriptionId = subscription.Id;
    TenantId = tenant.Id;
  }

  public DateTime ExpiryDate { get; private set; }

  public Subscription Subscription { get; set; }
  public Guid SubscriptionId { get; set; }

  public FSHTenantInfo Tenant { get; set; }
  public string TenantId { get; set; }

  public IReadOnlyCollection<SubscriptionHistory> History => _history.AsReadOnly();

  public TenantSubscription Renew(DateTime today)
  {
    _history.Add(new SubscriptionHistory(Id, today, Subscription.Days, Subscription.Price));
    ExpiryDate = today.AddDays(Subscription.Days);
    return this;
  }
}

public sealed class SubscriptionHistory : AuditableEntity
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