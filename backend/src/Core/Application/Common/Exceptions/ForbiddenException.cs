using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class ForbiddenException : CustomException
{
  public ForbiddenException(string message)
    : base(message, null, HttpStatusCode.Forbidden)
  {
  }
}