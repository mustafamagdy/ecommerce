using System.Collections.ObjectModel;
using System.Net;

namespace FSH.WebApi.Shared.Exceptions;

public sealed class InvalidCashRegisterOperation : CustomException
{
  public InvalidCashRegisterOperation(string message, ReadOnlyCollection<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.FailedDependency)
    : base(message, errors, statusCode)
  {
  }
}