using FSH.WebApi.Application.Common.Interfaces;

namespace FSH.WebApi.Infrastructure.Common.Services;

public sealed class DefaultSystemTime : ISystemTime
{
  public DateTime Now => DateTime.Now;
  public DateTime UtcNow => DateTime.UtcNow;
}