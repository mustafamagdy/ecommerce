using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.Structure;

public class Branch : AuditableEntity, IAggregateRoot
{
  private Branch()
  {
  }

  public string TenantId { get; set; }
  public FSHTenantInfo Tenant { get; set; }

  public virtual HashSet<CashRegister> CashRegisters { get; set; }

  public string Name { get; set; }
  public string? Description { get; set; }

  public Branch(string tenantId, string name, string? description)
  {
    TenantId = tenantId;
    Name = name;
    Description = description;
  }

  public Branch Update(string? name, string? description)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    return this;
  }
}