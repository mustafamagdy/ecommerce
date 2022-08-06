using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Finance;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSH.WebApi.Infrastructure.Finance;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireOpenCashRegisterHeaderAttribute : Attribute
{
}

public class RequireOpenCashRegisterFilter : IAsyncActionFilter
{
  private readonly ICashRegisterResolver _cashRegisterResolver;
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public RequireOpenCashRegisterFilter(ICashRegisterResolver cashRegisterResolver, IHttpContextAccessor httpContextAccessor,
    IReadRepository<CashRegister> cashRegisterRepo)
  {
    _cashRegisterResolver = cashRegisterResolver;
    _httpContextAccessor = httpContextAccessor;
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
      var cashRegisterId = await _cashRegisterResolver.Resolve(_httpContextAccessor.HttpContext);
      var cashRegister = await _cashRegisterRepo.GetByIdAsync(cashRegisterId);
      if (!cashRegister.Opened)
      {
        throw new InvalidCashRegisterOperation("Cash register is not opened for operations");
      }

      await next();
    }
  }
}