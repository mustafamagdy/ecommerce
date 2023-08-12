using System.Collections.Specialized;
using FSH.WebApi.Application.Operation.CurrentBranchs;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace FSH.WebApi.Application.Multitenancy;

public class CurrentBranchResolver : ICurrentBranchResolver
{
  public Guid Resolve(object context)
  {
    if (context is not HttpContext && context is not NameValueCollection)
    {
      throw new ArgumentException("Current branch cannot be resolved");
    }

    var httpContext = context as HttpContext ?? throw new InvalidCastException("Object cannot be casted as HttpContext");
    var branchIdClaim = httpContext.User?.Claims.FirstOrDefault(c => c.Type == FSHClaims.Branch);
    if (branchIdClaim == null)
    {
      throw new MissingHeaderException("Current branch not found in user claims");
    }

    if (!Guid.TryParse(branchIdClaim.Value, out var branchId))
    {
      throw new MissingHeaderException("Current branch not found in user claims");
    }

    return branchId;
  }
}