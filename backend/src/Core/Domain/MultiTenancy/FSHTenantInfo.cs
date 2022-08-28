using System.ComponentModel.DataAnnotations.Schema;
using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Internal;

namespace FSH.WebApi.Domain.MultiTenancy;

public interface ITenantConnectionStrings
{
  string? ConnectionString { get; set; }
  string? DemoConnectionString { get; set; }
  string? TrainConnectionString { get; set; }
}

public class FSHTenantInfo : ITenantInfo, ITenantConnectionStrings, IEntity
{
  private readonly List<DomainEvent> _domainEvents = new();

  public FSHTenantInfo()
  {
  }

  public FSHTenantInfo(string id, string name, string? connectionString, string? demoConnectionString, string? trainConnectionString,
    string adminEmail, string? phoneNumber, string? vatNo, string? email, string? address, string? adminName,
    string? adminPhoneNumber, string? techSupportUserId, string? issuer = null)
  {
    Id = id;
    Identifier = id;
    Name = name;
    ConnectionString = connectionString ?? string.Empty;
    DemoConnectionString = demoConnectionString ?? string.Empty;
    TrainConnectionString = trainConnectionString ?? string.Empty;
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
  public string Id { get; private set; }

  /// <summary>
  /// The identifier that is used in headers/routes/querystrings. This is set to the same as Id to avoid confusion.
  /// </summary>
  public string Identifier { get; private set; }

  public string Name { get; private set; }

  public string AdminEmail { get; private set; }
  public string? PhoneNumber { get; private set; }
  public string? VatNo { get; private set; }
  public string? Email { get; private set; }
  public string? Address { get; private set; }
  public string? AdminName { get; private set; }
  public string? AdminPhoneNumber { get; private set; }
  public string? TechSupportUserId { get; private set; }
  public bool Active { get; private set; }

  public void SetProdSubscription(TenantProdSubscription subscription)
  {
    ProdSubscription = subscription;
    ProdSubscriptionId = subscription.Id;
  }

  public TenantProdSubscription? ProdSubscription { get; private set; }
  public Guid? ProdSubscriptionId { get; private set; }
  public string? ConnectionString { get; set; }

  public TenantDemoSubscription? DemoSubscription { get; private set; }
  public Guid? DemoSubscriptionId { get; private set; }
  public string? DemoConnectionString { get; set; }

  public TenantTrainSubscription? TrainSubscription { get; private set; }
  public Guid? TrainSubscriptionId { get; private set; }
  public string? TrainConnectionString { get; set; }

  public decimal TotalDue => ProdSubscription?.TotalDue ?? 0;
  public decimal TotalPaid => ProdSubscription?.TotalPaid ?? 0;

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
  public string? Issuer { get; private set; }

  public string? Key => Name?.ToLower().Replace(" ", "-");

  string? ITenantInfo.Id { get => Id; set => Id = value ?? throw new InvalidOperationException("Id can't be null."); }

  string? ITenantInfo.Identifier { get => Identifier; set => Identifier = value ?? throw new InvalidOperationException("Identifier can't be null."); }

  string? ITenantInfo.Name { get => Name; set => Name = value ?? throw new InvalidOperationException("Name can't be null."); }

  [NotMapped]
  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public void AddDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
  public void ClearDomainEvents() => _domainEvents.Clear();
}