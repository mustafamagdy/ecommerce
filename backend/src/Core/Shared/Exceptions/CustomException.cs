using System.Collections.ObjectModel;
using System.Net;

namespace FSH.WebApi.Shared.Exceptions;

public abstract class CustomException : Exception
{
  public ReadOnlyCollection<string>? ErrorMessages { get; }

  public HttpStatusCode StatusCode { get; }

  protected CustomException(string message, ReadOnlyCollection<string>? errors = default,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : base(message)
  {
    ErrorMessages = errors;
    StatusCode = statusCode;
  }

  protected CustomException()
  {
  }

  protected CustomException(string message, Exception innerException)
    : base(message, innerException)
  {
  }

  protected CustomException(string message)
    : base(message)
  {
  }
}