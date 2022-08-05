using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Common.Exceptions;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSH.WebApi.Infrastructure.Finance;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireOpenCashRegisterHeaderAttribute : Attribute
{
}

public class RequireOpenCashRegisterFilter : IAsyncActionFilter
{
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;

  public RequireOpenCashRegisterFilter(IReadRepository<CashRegister> cashRegisterRepo)
  {
    _cashRegisterRepo = cashRegisterRepo;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
    var cashHeaderAtt = typeof(RequireOpenCashRegisterHeaderAttribute);
    var notHasAtt = descriptor.MethodInfo.GetCustomAttributes(cashHeaderAtt, true).Length > 0;
    if (!notHasAtt)
    {
      await next();
    }
    else
    {
      if (!context.HttpContext.Request.Headers.TryGetValue(MultitenancyConstants.CashRegisterHeaderName, out var headerValues)
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

      if (!cashRegister.Opened)
      {
        throw new InvalidCashRegisterOperation("Cash register is not opened for operations");
      }

      await next();
    }
  }
}