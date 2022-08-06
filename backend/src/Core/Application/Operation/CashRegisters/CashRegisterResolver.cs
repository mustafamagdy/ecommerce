using System.Collections.Specialized;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Finance;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class CashRegisterResolver : ICashRegisterResolver
{
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;

  public CashRegisterResolver(IReadRepository<CashRegister> cashRegisterRepo)
  {
    _cashRegisterRepo = cashRegisterRepo;
  }

  public async Task<CashRegister> Resolve(object context)
  {
    if (context is not HttpContext && context is not NameValueCollection)
    {
      throw new ArgumentException("Cash register cannot be resolved");
    }

    var httpContext = context as HttpContext ?? throw new InvalidCastException("Object cannot be casted as HttpContext");

    if (!httpContext.Request.Headers.TryGetValue(MultitenancyConstants.CashRegisterHeaderName, out var headerValues)
        || headerValues.Count == 0)
    {
      throw new MissingHeaderException("Cash register header is not provided for operation");
    }

    if (!Guid.TryParse(headerValues[0], out var cashRegisterId))
    {
      throw new MissingHeaderException("Cash register header is not provided for operation");
    }

    var cashRegister = await _cashRegisterRepo.GetByIdAsync(cashRegisterId);
    if (cashRegister == null)
    {
      throw new NotFoundException("Cash register provided in header is not valid");
    }

    return cashRegister;
  }
}