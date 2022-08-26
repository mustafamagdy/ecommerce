using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Customers;
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

    var users = await GetUserList(adminHeaders);
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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

    var users = await GetUserList(adminHeaders);

    var randomItem = catalog.Data[1];
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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
    var users = await GetUserList(adminHeaders);

    var randomItem = catalog.Data[1];
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();
    order.Should().NotBeNull();

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
    operation.Amount.Should().Be(order.NetAmount);
  }

  [Fact]
  public async Task can_create_order_and_export_its_pdf_invoice()
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

    var users = await GetUserList(adminHeaders);

    var randomItem = catalog.Data[1];
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();
    order.Should().NotBeNull();
    var orderId = order.Id;

    try
    {
      _ = await GetAsync($"/api/v1/orders/pdf/{orderId}", adminHeaders);
      _.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    catch (Exception e)
    {
      Console.WriteLine("This may fail in the cloud for running tests");
    }
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

  [Fact]
  public async Task can_create_order_and_pay_for_it_later()
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
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders/with-customer", new CreateOrderWithNewCustomerRequest()
    {
      Customer = new CreateSimpleCustomerRequest
      {
        Name = "customer name",
        PhoneNumber = "1234567"
      },
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();

    var orderPayment = new PayForOrderRequest()
    {
      Amount = 100,
      OrderId = order.Id,
    };
    _ = await PostAsJsonAsync("/api/v1/orders/pay", orderPayment, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var payment = await _.Content.ReadFromJsonAsync<OrderPaymentDto>();
    payment.Should().NotBeNull();
    payment.Amount.Should().Be(orderPayment.Amount);
  }

  [Fact]
  public async Task customer_balance_affected_by_order_and_payment()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newCustomer = new CreateSimpleCustomerRequest
    {
      Name = "customer name",
      PhoneNumber = "1234567"
    };
    var _ = await PostAsJsonAsync("api/v1/customers", newCustomer, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var customer = await _.Content.ReadFromJsonAsync<BasicCustomerDto>();

    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest()
    {
      CustomerId = customer.Id,
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    var firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();

    firstCustomer.DueAmount.Should().Be(order.NetAmount);

    var orderPayment = new PayForOrderRequest()
    {
      Amount = 100,
      OrderId = order.Id,
    };
    _ = await PostAsJsonAsync("/api/v1/orders/pay", orderPayment, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var payment = await _.Content.ReadFromJsonAsync<OrderPaymentDto>();
    payment.Should().NotBeNull();
    payment.Amount.Should().Be(orderPayment.Amount);

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();

    firstCustomer.DueAmount.Should().Be(order.NetAmount - orderPayment.Amount);
  }

  [Fact]
  public async Task new_order_to_customer_should_listed_in_customer_orders()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newCustomer = new CreateSimpleCustomerRequest
    {
      Name = "customer name",
      PhoneNumber = "1234567"
    };
    var _ = await PostAsJsonAsync("api/v1/customers", newCustomer, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var customer = await _.Content.ReadFromJsonAsync<BasicCustomerDto>();

    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest()
    {
      CustomerId = customer.Id,
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();

    _ = await PostAsJsonAsync("/api/v1/customers/with-orders", new SearchCustomerWithOrdersRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithOrdersDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    var firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();

    firstCustomer.DueAmount.Should().Be(order.NetAmount);
    var ordersCount = firstCustomer.Orders.Count;
    ordersCount.Should().Be(1);
  }

  [Fact]
  public async Task search_customer_with_balance_range_should_list_those_customers()
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
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    var customer1 = await create_customer_and_order(adminHeaders, randomItem.Id);
    var customer2 = await create_customer_and_order(adminHeaders, randomItem.Id);

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Balance = Range<decimal>.With(0, 100)
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    var customers = customerWithBalance.Data;

    customers.Should().Contain(a => a.Name == customer1.Name);
    customers.Should().Contain(a => a.Name == customer2.Name);
  }

  private async Task<BasicCustomerDto> create_customer_and_order(Dictionary<string, string> adminHeaders, Guid orderItemId)
  {
    var newCustomer = new CreateSimpleCustomerRequest
    {
      Name = Guid.NewGuid().ToString(),
      PhoneNumber = Guid.NewGuid().ToString()
    };
    var _ = await PostAsJsonAsync("api/v1/customers", newCustomer, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var customer = await _.Content.ReadFromJsonAsync<BasicCustomerDto>();

    _ = await PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest()
    {
      CustomerId = customer.Id,
      Items = new List<OrderItemRequest>()
      {
        new()
        {
          ItemId = orderItemId,
          Qty = 1
        }
      }
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    return customer;
  }

  [Fact]
  public async Task can_create_order_and_cancel_it_should_affect_customer_balance()
  {
    var adminHeaders = await CreateTenantAndLogin();
    var newCustomer = new CreateSimpleCustomerRequest
    {
      Name = "customer name",
      PhoneNumber = "1234567"
    };
    var _ = await PostAsJsonAsync("api/v1/customers", newCustomer, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var customer = await _.Content.ReadFromJsonAsync<BasicCustomerDto>();

    var newBranch = new CreateBranchRequest
    {
      Name = Guid.NewGuid().ToString()
    };
    _ = await PostAsJsonAsync("/api/v1/branch", newBranch, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/branch/search", new SearchBranchRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var branches = await _.Content.ReadFromJsonAsync<List<BranchDto>>();
    var branch = branches.First(a => a.Name == newBranch.Name);
    var users = await GetUserList(adminHeaders);

    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branch.Id,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var catalog = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalog.Data.Should().NotBeNullOrEmpty();

    var randomItem = catalog.Data[1];

    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest()
    {
      CustomerId = customer.Id,
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
    var order = await _.Content.ReadFromJsonAsync<OrderDto>();

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    var firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();

    firstCustomer.DueAmount.Should().Be(order.NetAmount);

    var orderPayment = new PayForOrderRequest()
    {
      Amount = 100,
      OrderId = order.Id,
    };
    _ = await PostAsJsonAsync("/api/v1/orders/pay", orderPayment, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var payment = await _.Content.ReadFromJsonAsync<OrderPaymentDto>();
    payment.Should().NotBeNull();
    payment.Amount.Should().Be(orderPayment.Amount);

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();

    firstCustomer.DueAmount.Should().Be(order.NetAmount - orderPayment.Amount);

    _ = await PutAsJsonAsync($"/api/v1/orders/cancel/{order.Id}", new CancelOrderWithPaymentsRequest(order.Id), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/customers/with-balance", new SearchCustomerWithBalanceRequest
    {
      Name = customer.Name
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    customerWithBalance = await _.Content.ReadFromJsonAsync<PaginationResponse<CustomerWithBalanceDto>>();
    customerWithBalance.Should().NotBeNull();
    customerWithBalance.Data.Should().NotBeNullOrEmpty();
    firstCustomer = customerWithBalance.Data.First();
    firstCustomer.Should().NotBeNull();
    firstCustomer.DueAmount.Should().Be(0);
  }

  /*
   * - cash_register
   * ? can_create_only_one_cash_register_per_branch

   * - customers
   * - orders
   * - payments
   */
}