using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Orders;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Operations;

public class OperationsTests : TestFixture
{
  public OperationsTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task can_create_cash_register_for_branch()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    branches.Should().NotBeNullOrEmpty();
    branches.Should().Contain(a => a.Name == newBranch.Name);
    var branch = branches.First(a => a.Name == newBranch.Name);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_do_any_operations_on_closed_cash_register()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/close", new CloseCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    adminHeaders.Add("cash-register", cashRegisterId.ToString());
    _ = await PostAsJsonAsync("/api/v1/orders/cash", new CreateCashOrderRequest()
    {
      Items = new List<OrderItemRequest>()
      {
        new()
        {
          ItemId = randomItem.Id,
          Qty = 1
        }
      }
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.FailedDependency);

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    // try to create the order again
    _ = await PostAsJsonAsync("/api/v1/orders/cash", new CreateCashOrderRequest()
    {
      Items = new List<OrderItemRequest>()
      {
        new()
        {
          ItemId = randomItem.Id,
          Qty = 1
        }
      }
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task can_open_cash_register()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task can_close_cash_register()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/close", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task any_payment_should_have_record_in_cash_register()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

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
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/cashRegister/search-active-operations", new SearchCashRegisterActiveOperationsRequest()
    {
      CashRegisterId = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var operations = await _.Content.ReadFromJsonAsync<PaginationResponse<CashRegisterActiveOperationDto>>();
    operations.Should().NotBeNull();
    operations.Data.Should().NotBeNullOrEmpty();
    operations.Data.Count.Should().Be(1);
    var operation = operations.Data[0];
    operation.Should().NotBeNull();
    operation.Amount.Should().Be(randomItem.Price * item.Qty);
  }

  [Fact]
  public async Task cash_register_active_operations_should_be_empty_when_open_cash_register()
  {
  }

  [Fact]
  public async Task cash_register_active_operations_should_be_empty_when_close_cash_register()
  {
  }

  [Fact]
  public async Task active_operations_should_moved_to_archived_operations_when_close_cash_register()
  {
  }

  [Fact]
  public async Task can_transfer_between_cash_register()
  {
  }

  [Fact]
  public async Task det_cash_register_had_to_accept_transfer_in_order_to_appear_in_its_balance()
  {
  }

  /*
   * - cash_register
   * ? can_create_only_one_cash_register_per_branch

   * - customers
   * - orders
   * - payments
   */
}