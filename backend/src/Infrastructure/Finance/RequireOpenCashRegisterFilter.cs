using System.Reflection;
using Ardalis.Specification;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Exceptions;
using FSH.WebApi.Shared.Finance;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSH.WebApi.Infrastructure.Finance;

public sealed class RequireOpenCashRegisterFilter : IAsyncActionFilter
{
  private readonly ICashRegisterResolver _cashRegisterResolver;
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;

  public RequireOpenCashRegisterFilter(ICashRegisterResolver cashRegisterResolver, IReadRepository<CashRegister> cashRegisterRepo)
  {
    _cashRegisterResolver = cashRegisterResolver;
    _cashRegisterRepo = cashRegisterRepo;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
    var attribute = descriptor.MethodInfo.GetCustomAttribute(typeof(RequireOpenedCashRegisterAttribute)) as RequireOpenedCashRegisterAttribute;
    if (attribute != null && string.Equals(attribute.HeaderName, MultitenancyConstants.CashRegisterHeaderName, StringComparison.CurrentCultureIgnoreCase))
    {
      var cashRegisterId = await _cashRegisterResolver.Resolve(context.HttpContext);
      var cashRegister = await _cashRegisterRepo.FirstOrDefaultAsync(new SingleResultSpecification<CashRegister>()
                           .Query
                           .Include(a => a.Branch)
                           .Where(a => a.Id == cashRegisterId)
                           .Specification)
                         ?? throw new NotFoundException($"Cash register {cashRegisterId} not found");

      if (!cashRegister.Branch.Active)
      {
        throw new InvalidCashRegisterOperation("The branch this cash register is defined into is not active");
      }

      if (!cashRegister.Opened)
      {
        throw new InvalidCashRegisterOperation("Cash register is not opened for operations");
      }

      await next();
    }
    else
    {
      await next();
    }
  }
}