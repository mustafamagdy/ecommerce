using System.Net;

namespace FSH.WebApi.Shared.Exceptions;

public abstract class CustomException : Exception
{
  public List<string>? ErrorMessages { get; }

  public HttpStatusCode StatusCode { get; }

  protected CustomException(string message, List<string>? errors = default, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : base(message)
  {
    ErrorMessages = errors;
    StatusCode = statusCode;
  }
}