using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class GetDefaultCashCustomerSpec : Specification<Customer>, ISingleResultSpecification
{
  public GetDefaultCashCustomerSpec() => Query.Where(a => a.CashDefault);
}