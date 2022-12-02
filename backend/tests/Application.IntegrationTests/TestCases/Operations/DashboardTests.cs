using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Application.Dashboard;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Application.Operation.Orders;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Operations;

public class DashboardTests : TestFixture
{
  public DashboardTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task dashboard_shows_order_on_correct_date()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();

    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();
    var users = await GetUserList(adminHeaders);

    var randomItem = catalog.Data[1];
    var cashRegisterId = await CreateNewCashRegister(branchId, users, adminHeaders);

    await OpenCashRegister(cashRegisterId, adminHeaders);

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    var item = new OrderItemRequest()
    {
      ItemId = randomItem.Id,
      Qty = 1
    };
    _ = await PostAsJsonAsync("/api/v1/orders/cash", new CreateCashOrderRequest()
    {
      Items = new List<OrderItemRequest>()
      {
        item
      }
    }, adminHeaders).ConfigureAwait(false);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();
    order.Should().NotBeNull();

    _ = await GetAsync("/api/v1/dashboard", adminHeaders).ConfigureAwait(false);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var dashboard = await _.Content.ReadFromJsonAsync<StatsDto>();
    dashboard.Should().NotBeNull();
    dashboard.BranchCount.Should().Be(1);
    dashboard.OrderCount.Should().Be(1);
    dashboard.DataEnterBarChart.Should().NotBeNullOrEmpty();
    var currentMonth = HostFixture.SYSTEM_TIME.Now.Month - 1;
    dashboard.DataEnterBarChart[0].Data[currentMonth].Should().Be(1);
    dashboard.DataEnterBarChart[1].Data[currentMonth].Should().Be((double)order.TotalAmount);
  }
}