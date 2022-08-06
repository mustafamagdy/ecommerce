using System.Net;

namespace FSH.WebApi.Shared.Exceptions;

public class InvalidCashRegisterOperation : CustomException
{
  public InvalidCashRegisterOperation(string message, List<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.FailedDependency)
    : base(message, errors, statusCode)
  {
  }
}