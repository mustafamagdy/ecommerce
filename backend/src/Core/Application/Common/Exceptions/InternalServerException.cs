using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class InternalServerException : CustomException
{
  public InternalServerException(string message, List<string>? errors = default)
    : base(message, errors)
  {
  }
}