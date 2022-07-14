using FSH.WebApi.Application.Common.Interfaces;

namespace Application.IntegrationTests.Infra;

public class TestSystemTime : ISystemTime
{
  public int DaysOffset = 0;
  public DateTime Now => DateTime.Now.AddDays(DaysOffset);
}