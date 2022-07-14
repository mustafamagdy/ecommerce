using FSH.WebApi.Application.Common.Interfaces;

namespace FSH.WebApi.Infrastructure.Common.Services;

public class DefaultSystemTime : ISystemTime
{
  public DateTime Now => DateTime.Now;
}