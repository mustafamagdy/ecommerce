using System.Net;
using FSH.WebApi.Shared.Exceptions;

namespace FSH.WebApi.Application.Common.Exceptions;

public class UnsupportedOperator : CustomException
{
  public UnsupportedOperator(string message)
    : base(message, null, HttpStatusCode.BadRequest)
  {
  }
}