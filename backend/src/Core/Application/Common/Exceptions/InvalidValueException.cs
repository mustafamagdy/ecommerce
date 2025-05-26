using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class InvalidValueException : CustomException
{
  public InvalidValueException(string message)
    : base(message, null, HttpStatusCode.BadRequest)
  {
  }
}