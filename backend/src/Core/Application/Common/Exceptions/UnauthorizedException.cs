using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class UnauthorizedException : CustomException
{
  public UnauthorizedException(string message)
    : base(message, null, HttpStatusCode.Unauthorized)
  {
  }
}