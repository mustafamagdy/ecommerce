using System.ComponentModel.DataAnnotations.Schema;
using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Domain.MultiTenancy;

public class FSHTenantInfo : ITenantInfo
{
  public FSHTenantInfo()
  {
  }

  public FSHTenantInfo(string id, string name, string? databaseName, string adminEmail, string? phoneNumber,
    string? vatNo, string? email, string? address, string? adminName, string? adminPhoneNumber,
    string? techSupportUserId, string? issuer = null)
  {
    Id = id;
    Identifier = id;
    Name = name;
    DatabaseName = databaseName ?? string.Empty;
    AdminEmail = adminEmail;
    PhoneNumber = phoneNumber;
    VatNo = vatNo;
    Email = email;
    Address = address;
    AdminName = adminName;
    AdminPhoneNumber = adminPhoneNumber;
    TechSupportUserId = techSupportUserId;
    IsActive = true;
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
  public string DatabaseName { get; set; } = default!;

  public string AdminEmail { get; private set; } = default!;
  public string? PhoneNumber { get; set; }
  public string? VatNo { get; set; }
  public string? Email { get; set; }
  public string? Address { get; set; }
  public string? AdminName { get; set; }
  public string? AdminPhoneNumber { get; set; }
  public string? TechSupportUserId { get; set; }
  public bool IsActive { get; private set; }

  public virtual HashSet<TenantSubscription> Subscriptions { get; set; } = default!;
  public virtual HashSet<Branch> Branches { get; set; } = default!;

  /// <summary>
  /// Used by AzureAd Authorization to store the AzureAd Tenant Issuer to map against.
  /// </summary>
  public string? Issuer { get; set; }

  public string? Key => Name?.ToLower().Replace(" ", "-");

  public void Activate()
  {
    if (Id == MultitenancyConstants.Root.Id)
    {
      throw new InvalidOperationException("Invalid Tenant");
    }

    IsActive = true;
  }

  public void Deactivate()
  {
    if (Id == MultitenancyConstants.Root.Id)
    {
      throw new InvalidOperationException("Invalid Tenant");
    }

    IsActive = false;
  }


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

  string? ITenantInfo.DatabaseName
  {
    get => DatabaseName;
    set => DatabaseName = value ?? throw new InvalidOperationException
      ("Database Name can't be null.");
  }
}