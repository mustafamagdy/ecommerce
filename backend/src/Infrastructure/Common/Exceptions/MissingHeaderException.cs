using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Infrastructure.Common.Exceptions;

public class MissingHeaderException : CustomException
{
  public MissingHeaderException(string message, List<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : base(message, errors, statusCode)
  {
  }
}