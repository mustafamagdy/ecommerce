using System.Data;
using Dapper;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Host.Controllers.Multitenancy;
using NSubstitute;
using Xunit;

namespace Application.Tests;

public class UnitTest1
{
  [Fact]
  public async Task Test1()
  {
    var tenant = new TenantDto();
    var db = Substitute.For<IDbConnection>();

    var qResult = Substitute.For<SqlMapper.GridReader>();
    qResult.IsConsumed.Returns(false);
    qResult
      .Read(Arg.Any<Func<TenantDto, TenantSubscriptionDto, SubscriptionPaymentDto, BranchDto, TenantDto>>(), Arg.Any<string>())
      .Returns(new[] { tenant });

    db.QueryMultipleAsync(Arg.Any<string>(), Arg.Any<object>()).Returns(qResult);
    var repo = Substitute.For<IDapperTenantConnectionAccessor>();
    repo.GetDbConnection().Returns(db);

    var handler = new SearchAllTenantsRequestHandler(repo);
    var result = await handler.Handle(new SearchAllTenantsRequest(), CancellationToken.None);
  }
}