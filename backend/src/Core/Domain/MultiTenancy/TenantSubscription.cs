namespace FSH.WebApi.Domain.MultiTenancy;

public sealed class TenantDemoSubscription : TenantSubscription
{
  private TenantDemoSubscription()
  {
  }

  public TenantDemoSubscription(SubscriptionPackage currentPackage, FSHTenantInfo tenant)
    : base(currentPackage, tenant)
  {
  }
}

public sealed class TenantTrainSubscription : TenantSubscription
{
  private TenantTrainSubscription()
  {
  }

  public TenantTrainSubscription(SubscriptionPackage currentPackage, FSHTenantInfo tenant)
    : base(currentPackage, tenant)
  {
  }
}

public sealed class TenantProdSubscription : TenantSubscription
{
  private readonly List<SubscriptionPayment> _payments = new();

  private TenantProdSubscription()
  {
  }

  public TenantProdSubscription(SubscriptionPackage currentPackage, FSHTenantInfo tenant)
    : base(currentPackage, tenant)
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

  protected TenantSubscription(SubscriptionPackage currentPackage, FSHTenantInfo tenant)
  {
    CurrentPackage = currentPackage;
    Tenant = tenant;
    CurrentPackageId = currentPackage.Id;
    TenantId = tenant.Id;
  }

  public DateTime ExpiryDate { get; private set; }

  public bool IsActive(DateTime now) => ExpiryDate >= now;
  public Guid CurrentPackageId { get; set; }
  public SubscriptionPackage CurrentPackage { get; set; }

  public string TenantId { get; set; }
  public FSHTenantInfo Tenant { get; set; }

  public IReadOnlyCollection<SubscriptionHistory> History => _history.AsReadOnly();

  public TenantSubscription Renew(DateTime today)
  {
    _history.Add(new SubscriptionHistory(Id, CurrentPackageId, today, CurrentPackage.ValidForDays, CurrentPackage.Price));
    ExpiryDate = today.AddDays(CurrentPackage.ValidForDays);
    return this;
  }
}

public sealed class SubscriptionHistory : AuditableEntity
{
  private SubscriptionHistory()
  {
  }

  public SubscriptionHistory(Guid tenantSubscriptionId, Guid packageId, DateTime startDate, int days, decimal price)
  {
    TenantSubscriptionId = tenantSubscriptionId;
    PackageId = packageId;
    StartDate = startDate;
    Price = price;
    ExpiryDate = startDate.AddDays(days);
  }

  public decimal Price { get; private set; }
  public DateTime StartDate { get; private set; }
  public DateTime ExpiryDate { get; private set; }

  public Guid PackageId { get; set; }
  public SubscriptionPackage Package { get; set; }

  public Guid TenantSubscriptionId { get; set; }
  public TenantSubscription TenantSubscription { get; set; }
}