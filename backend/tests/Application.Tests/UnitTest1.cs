using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Host.Controllers.Multitenancy;
using Xunit;

namespace Application.Tests;

public class UnitTest1
{
  [Fact]
  public async Task Test1()
  {
    var sut = new TenantsController();
    var result = await sut.GetListAsync(new SearchAllTenantsRequest());
  }
}