using Finbuckle.MultiTenant;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Domain.MultiTenancy;

public class FSHTenantInfo : ITenantInfo
{
  public FSHTenantInfo()
  {
  }

  public FSHTenantInfo(string id, string name, string? connectionString, string adminEmail, string? phoneNumber,
    string? vatNo, string? email, string? address, string? adminName, string? adminPhoneNumber,
    string? techSupportUserId, string? issuer = null)
  {
    Id = id;
    Identifier = id;
    Name = name;
    ConnectionString = connectionString ?? string.Empty;
    AdminEmail = adminEmail;
    PhoneNumber = phoneNumber;
    VatNo = vatNo;
    Email = email;
    Address = address;
    AdminName = adminName;
    AdminPhoneNumber = adminPhoneNumber;
    TechSupportUserId = techSupportUserId;
    Active = true;
    Issuer = issuer;
  }

  /// <summary>
  /// The actual TenantId, which is also used in the TenantId shadow property on the multitenant entities.
  /// </summary>
  public string Id { get; set; } = default!;

  /// <summary>
  /// The identifier that is used in headers/routes/querystrings. This is set to the same as Id to avoid confusion.
  /// </summary>
  public string Identifier { get; set; } = default!;

  public string Name { get; set; } = default!;

  public string AdminEmail { get; private set; } = default!;
  public string? PhoneNumber { get; set; }
  public string? VatNo { get; set; }
  public string? Email { get; set; }
  public string? Address { get; set; }
  public string? AdminName { get; set; }
  public string? AdminPhoneNumber { get; set; }
  public string? TechSupportUserId { get; set; }
  public bool Active { get; private set; }

  public StandardSubscription? ProdSubscription { get; set; }
  public Guid? ProdSubscriptionId { get; set; }
  public string? ConnectionString { get; set; }

  public DemoSubscription? DemoSubscription { get; set; }
  public Guid? DemoSubscriptionId { get; set; }
  public string? DemoConnectionString { get; set; }

  public TrainSubscription? TrainSubscription { get; set; }
  public Guid? TrainSubscriptionId { get; set; }
  public string? TrainConnectionString { get; set; }


  public virtual HashSet<SubscriptionPayment> Payments { get; set; } = default!;

  public DateTime StartDate { get; private set; }
  public DateTime ExpiryDate { get; private set; }

  public decimal TotalPaid => Payments?.Sum(a => a.Amount) ?? 0;
  public decimal Balance => (ProdSubscription?.SubscriptionHistory.Sum(a => a.Price) ?? 0) - TotalPaid;

  public void Pay(decimal amount, Guid paymentMethodId)
  {
    if (ProdSubscriptionId == null)
    {
      throw new NullReferenceException("No valid prod subscription to renew");
    }

    Payments.Add(new SubscriptionPayment(amount, paymentMethodId).SetSubscription(ProdSubscriptionId.Value));
  }

  public FSHTenantInfo Renew()
  {
    if (ProdSubscription == null)
    {
      throw new NullReferenceException("No valid prod subscription to renew");
    }

    var today = DateTime.Now;
    ProdSubscription.SubscriptionHistory.Add(new SubscriptionHistory
    {
      TenantId = Id,
      Price = ProdSubscription.Price,
      StartDate = today,
      ExpiryDate = today.AddDays(ProdSubscription.Days)
    });
    return Activate();
  }

  public FSHTenantInfo Activate()
  {
    if (Id == MultitenancyConstants.Root.Id)
    {
      throw new InvalidOperationException("Invalid Tenant");
    }

    Active = true;
    return this;
  }

  public FSHTenantInfo DeActivate()
  {
    if (Id == MultitenancyConstants.Root.Id)
    {
      throw new InvalidOperationException("Invalid Tenant");
    }

    Active = false;
    return this;
  }

  /// <summary>
  /// Used by AzureAd Authorization to store the AzureAd Tenant Issuer to map against.
  /// </summary>
  public string? Issuer { get; set; }

  public string? Key => Name?.ToLower().Replace(" ", "-");

  string? ITenantInfo.Id
  {
    get => Id;
    set => Id = value ?? throw new InvalidOperationException("Id can't be null.");
  }

  string? ITenantInfo.Identifier
  {
    get => Identifier;
    set => Identifier = value ?? throw new InvalidOperationException("Identifier can't be null.");
  }

  string? ITenantInfo.Name
  {
    get => Name;
    set => Name = value ?? throw new InvalidOperationException("Name can't be null.");
  }
}