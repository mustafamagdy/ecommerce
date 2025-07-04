﻿using FSH.WebApi.Application.Operation.Customers;
using FSH.WebApi.Infrastructure.Finance;

namespace FSH.WebApi.Host.Controllers.Operation;

public sealed class CustomersController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Customers)]
  [OpenApiOperation("Search customers using available filters.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<PaginationResponse<BasicCustomerDto>> SearchAsync(SearchCustomersRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.Search, FSHResource.Customers)]
  [OpenApiOperation("Get customer by id.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<BasicCustomerDto> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return Mediator.Send(new GetCustomerByIdRequest(id), cancellationToken);
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Search, FSHResource.Customers)]
  [OpenApiOperation("Delete customer by id.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Search))]
  public Task<Guid> DeleteCustomerIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return Mediator.Send(new DeleteCustomerByIdRequest(id), cancellationToken);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Customers)]
  [OpenApiOperation("Create a new customer.", "")]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create1))]
  public Task<BasicCustomerDto> CreateCustomer(CreateSimpleCustomerRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost("with-balance")]
  [MustHavePermission(FSHAction.Search, FSHResource.Customers)]
  [OpenApiOperation("Search customers with filters and balance.", "")]
  [RequireOpenedCashRegister]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create1))]
  public Task<PaginationResponse<CustomerWithBalanceDto>> SearchWithBalance(SearchCustomerWithBalanceRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }

  [HttpPost("with-orders")]
  [MustHavePermission(FSHAction.Search, FSHResource.Customers)]
  [OpenApiOperation("Search customers with filters and balance and get their orders.", "")]
  [RequireOpenedCashRegister]
  [ApiConventionMethod(typeof(FSHApiConventions), nameof(FSHApiConventions.Create))]
  public Task<PaginationResponse<CustomerWithOrdersDto>> CreateWithCustomer(SearchCustomerWithOrdersRequest request, CancellationToken cancellationToken)
  {
    return Mediator.Send(request, cancellationToken);
  }
}