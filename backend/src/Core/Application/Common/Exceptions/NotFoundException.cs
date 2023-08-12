using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class NotFoundException : CustomException
{
  public NotFoundException(string message)
    : base(message, null, HttpStatusCode.NotFound)
  {
  }
}