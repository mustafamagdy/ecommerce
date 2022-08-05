using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Infrastructure.Common.Exceptions;

public class InvalidCashRegisterOperation : CustomException
{
  public InvalidCashRegisterOperation(string message, List<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.FailedDependency)
    : base(message, errors, statusCode)
  {
  }
}