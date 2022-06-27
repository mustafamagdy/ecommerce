using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public sealed class CashRegisterByNameSpec : Specification<CashRegister>, ISingleResultSpecification
{
  public CashRegisterByNameSpec(string name) => Query.Where(b => b.Name == name);
}