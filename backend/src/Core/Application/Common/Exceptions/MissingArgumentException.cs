using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class MissingArgumentException : CustomException
{
  public MissingArgumentException(string message)
    : base(message, null, HttpStatusCode.BadRequest)
  {
  }
}