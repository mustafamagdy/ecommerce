using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class ConflictException : CustomException
{
  public ConflictException(string message)
    : base(message, null, HttpStatusCode.Conflict)
  {
  }
}