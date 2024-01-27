using System.Collections.ObjectModel;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Domain.Structure;

public sealed class Branch : AuditableEntity, IAggregateRoot
{
  private readonly List<CashRegister> _cashRegisters = new();

  private Branch()
  {
  }

  public Branch(string tenantId, string name, string? description)
  {
    TenantId = tenantId;
    Name = name;
    Description = description;
  }

  public bool Active { get; private set; } = true;
  public string Name { get; private set; }
  public string? Description { get; private set; }

  public string TenantId { get; set; }
  public FSHTenantInfo Tenant { get; private set; }

  public IReadOnlyCollection<CashRegister> CashRegisters => _cashRegisters.AsReadOnly();

  public void AddCashRegister(CashRegister cashRegister) => _cashRegisters.Add(cashRegister);

  public Branch Update(string? name, string? description)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    return this;
  }

  public void Activate()
  {
    if (Active) return;

    Active = true;
    AddDomainEvent(new BranchActivatedEvent(this));
  }

  public void Deactivate()
  {
    if (!Active) return;

    Active = false;
    AddDomainEvent(new BranchDeactivatedEvent(this));
  }
}