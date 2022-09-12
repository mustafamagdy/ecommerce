using System.Net;

namespace FSH.WebApi.Shared.Exceptions;

public sealed class MissingHeaderException : CustomException
{
  public MissingHeaderException(string message, List<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : base(message, errors, statusCode)
  {
  }
}